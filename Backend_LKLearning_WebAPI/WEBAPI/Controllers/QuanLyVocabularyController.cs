using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPI.DTOS;
using WEBAPI.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyVocabularyController : ControllerBase
    {
        private readonly LuanvantienganhContext db;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public QuanLyVocabularyController(LuanvantienganhContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            db = context;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        // Lấy thông tin từ vựng từ cơ sở dữ liệu hoặc Free Dictionary API và tự động lưu nếu chưa có
        [HttpGet("get-word/{word}")]
        public async Task<IActionResult> GetWordDefinition(string word)
        {
            try
            {
                // Kiểm tra xem từ vựng đã có trong cơ sở dữ liệu hay chưa
                var existingVocabulary = await db.Vocabularies
                    .Include(v => v.VocabularyMeanings)
                    .FirstOrDefaultAsync(v => v.Word.ToLower() == word.ToLower());

                if (existingVocabulary != null)
                {
                    var vocabularyDTO = new VocabularyDTO
                    {
                        VocabularyId = existingVocabulary.VocabularyId,
                        Word = existingVocabulary.Word,
                        Pronunciation = existingVocabulary.Pronunciation,
                        AudioUrlUk = existingVocabulary.AudioUrlUk,
                        AudioUrlUs = existingVocabulary.AudioUrlUs,
                        CreatedAt = existingVocabulary.CreatedAt,
                        Meanings = existingVocabulary.VocabularyMeanings.Select(m => new VocabularyMeaningDTO
                        {
                            VocabularyMeaningId = m.VocabularyMeaningId,
                            VocabularyId = m.VocabularyId,
                            Meaning = m.Meaning,
                            ExampleSentence = m.ExampleSentence,
                            TranslatedMeaning = m.TranslatedMeaning,
                            TranslatedExampleSentence = m.TranslatedExampleSentence,
                            Synonyms = m.Synonyms,
                            Antonyms = m.Antonyms,
                            PartOfSpeech = m.PartOfSpeech
                        }).ToList()
                    };
                    return Ok(vocabularyDTO);
                }

                // Kiểm tra trùng lặp trước khi gọi API
                var existingWord = await db.Vocabularies.AnyAsync(v => v.Word.ToLower() == word.ToLower());
                if (existingWord)
                {
                    return Conflict(new { message = $"Từ '{word}' đã tồn tại trong cơ sở dữ liệu." });
                }

                // Gọi Free Dictionary API
                var response = await _httpClient.GetAsync($"https://api.dictionaryapi.dev/api/v2/entries/en/{word}");
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound(new { message = $"Không tìm thấy từ '{word}'" });
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JArray.Parse(json)[0];

                var meanings = new List<VocabularyMeaningDTO>();
                var textsToTranslate = new List<string>();

                // Xử lý từng partOfSpeech và các definitions
                foreach (var meaning in data["meanings"])
                {
                    var partOfSpeech = meaning["partOfSpeech"]?.ToString();
                    foreach (var definition in meaning["definitions"])
                    {
                        var meaningText = definition["definition"]?.ToString();
                        var exampleText = definition["example"]?.ToString();
                        var synonyms = string.Join(",", definition["synonyms"]?.Values<string>() ?? new List<string>());
                        var antonyms = string.Join(",", definition["antonyms"]?.Values<string>() ?? new List<string>());

                        if (!string.IsNullOrEmpty(meaningText) && !meaningText.Contains("translate=\"no\""))
                            textsToTranslate.Add(meaningText);
                        if (!string.IsNullOrEmpty(exampleText) && !exampleText.Contains("translate=\"no\""))
                            textsToTranslate.Add(exampleText);

                        meanings.Add(new VocabularyMeaningDTO
                        {
                            Meaning = meaningText,
                            ExampleSentence = exampleText,
                            PartOfSpeech = partOfSpeech,
                            Synonyms = synonyms,
                            Antonyms = antonyms
                        });
                    }
                }

                // Dịch sang tiếng Việt
                if (textsToTranslate.Any())
                {
                    var translatedTexts = await TranslateTexts(textsToTranslate, "vi");
                    int index = 0;
                    foreach (var meaning in meanings)
                    {
                        if (!string.IsNullOrEmpty(meaning.Meaning))
                        {
                            if (index < translatedTexts.Count)
                            {
                                meaning.TranslatedMeaning = translatedTexts[index];
                                index++;
                            }
                            else
                            {
                                meaning.TranslatedMeaning = "Dịch không thành công";
                            }
                        }
                        if (!string.IsNullOrEmpty(meaning.ExampleSentence))
                        {
                            if (index < translatedTexts.Count)
                            {
                                meaning.TranslatedExampleSentence = translatedTexts[index];
                                index++;
                            }
                            else
                            {
                                meaning.TranslatedExampleSentence = "Dịch không thành công";
                            }
                        }
                    }
                }

                // Phân biệt AudioUrlUK và AudioUrlUS
                var audioUk = data["phonetics"]?.FirstOrDefault(p => p["audio"]?.ToString().Contains("uk") ?? false)?["audio"]?.ToString();
                var audioUs = data["phonetics"]?.FirstOrDefault(p => p["audio"]?.ToString().Contains("us") ?? false)?["audio"]?.ToString();
                var pronunciation = data["phonetics"]?.FirstOrDefault(p => !string.IsNullOrEmpty(p["text"]?.ToString()))?["text"]?.ToString();

                var vocabulary = new Vocabulary
                {
                    Word = word.ToLower(),
                    Pronunciation = pronunciation,
                    AudioUrlUk = audioUk,
                    AudioUrlUs = audioUs,
                    CreatedAt = DateTime.Now
                };

                db.Vocabularies.Add(vocabulary);
                await db.SaveChangesAsync();

                foreach (var meaningDto in meanings)
                {
                    var meaning = new VocabularyMeaning
                    {
                        VocabularyId = vocabulary.VocabularyId,
                        Meaning = meaningDto.Meaning,
                        ExampleSentence = meaningDto.ExampleSentence,
                        TranslatedMeaning = meaningDto.TranslatedMeaning,
                        TranslatedExampleSentence = meaningDto.TranslatedExampleSentence,
                        Synonyms = meaningDto.Synonyms,
                        Antonyms = meaningDto.Antonyms,
                        PartOfSpeech = meaningDto.PartOfSpeech
                    };
                    db.VocabularyMeanings.Add(meaning);
                }
                await db.SaveChangesAsync();

                var vocabularyDto = new VocabularyDTO
                {
                    VocabularyId = vocabulary.VocabularyId,
                    Word = vocabulary.Word,
                    Pronunciation = vocabulary.Pronunciation,
                    AudioUrlUk = vocabulary.AudioUrlUk,
                    AudioUrlUs = vocabulary.AudioUrlUs,
                    CreatedAt = vocabulary.CreatedAt,
                    Meanings = meanings
                };

                return Ok(vocabularyDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gọi API: " + ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "") });
            }
        }
        // Hàm dịch nhiều văn bản sang ngôn ngữ đích qua AI Translate API trên RapidAPI
        private async Task<List<string>> TranslateTexts(List<string> texts, string targetLanguage)
        {
            if (!texts.Any()) return new List<string>();

            var requestBody = new
            {
                texts = texts.ToArray(),
                tl = targetLanguage,
                sl = "auto"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _configuration["RapidApi:TranslateUrl"])
            {
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-RapidAPI-Key", _configuration["RapidApi:ApiKey"]);
            request.Headers.Add("X-RapidAPI-Host", _configuration["RapidApi:ApiHost"]);

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(json);
                return result["texts"]?.Values<string>().ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API Translate: {ex.Message}");
                return new List<string>();
            }
        }

        [HttpPost("CreateVocabulary")]
        public async Task<IActionResult> CreateVocabulary([FromBody] VocabularyDTO vocabularyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Dữ liệu không hợp lệ");

            // Kiểm tra từ đã tồn tại (theo Word, không phân biệt hoa thường)
            var existingVocabulary = await db.Vocabularies
                .Include(v => v.VocabularyMeanings)
                .FirstOrDefaultAsync(v => v.Word.ToLower() == vocabularyDto.Word.ToLower());

            if (existingVocabulary != null)
            {
                // Nếu đã có, cập nhật thông tin
                existingVocabulary.Pronunciation = vocabularyDto.Pronunciation;
                existingVocabulary.AudioUrlUk = vocabularyDto.AudioUrlUk;
                existingVocabulary.AudioUrlUs = vocabularyDto.AudioUrlUs;

                // Xóa meanings cũ
                db.VocabularyMeanings.RemoveRange(existingVocabulary.VocabularyMeanings);

                // Thêm meanings mới
                foreach (var m in vocabularyDto.Meanings)
                {
                    db.VocabularyMeanings.Add(new VocabularyMeaning
                    {
                        VocabularyId = existingVocabulary.VocabularyId,
                        Meaning = m.Meaning,
                        ExampleSentence = m.ExampleSentence,
                        TranslatedMeaning = m.TranslatedMeaning,
                        TranslatedExampleSentence = m.TranslatedExampleSentence,
                        Synonyms = m.Synonyms,
                        Antonyms = m.Antonyms,
                        PartOfSpeech = m.PartOfSpeech
                    });
                }
                await db.SaveChangesAsync();

                vocabularyDto.VocabularyId = existingVocabulary.VocabularyId;
                return Ok(new { message = "Cập nhật thành công", data = vocabularyDto });
            }

            // Nếu chưa có, thêm mới như cũ
            var vocabulary = new Vocabulary
            {
                Word = vocabularyDto.Word,
                Pronunciation = vocabularyDto.Pronunciation,
                AudioUrlUk = vocabularyDto.AudioUrlUk,
                AudioUrlUs = vocabularyDto.AudioUrlUs,
                CreatedAt = DateTime.Now
            };

            // Thêm vocabulary và lưu
            db.Vocabularies.Add(vocabulary);
            var savedCount = await db.SaveChangesAsync();
            if (savedCount <= 0)
                return StatusCode(500, "Thêm từ vựng thất bại");

            // Thêm meanings
            foreach (var m in vocabularyDto.Meanings)
            {
                db.VocabularyMeanings.Add(new VocabularyMeaning
                {
                    VocabularyId = vocabulary.VocabularyId,
                    Meaning = m.Meaning,
                    ExampleSentence = m.ExampleSentence,
                    TranslatedMeaning = m.TranslatedMeaning,
                    TranslatedExampleSentence = m.TranslatedExampleSentence,
                    Synonyms = m.Synonyms,
                    Antonyms = m.Antonyms,
                    PartOfSpeech = m.PartOfSpeech
                });
            }
            savedCount = await db.SaveChangesAsync();
            if (savedCount <= 0)
                return StatusCode(500, "Thêm nghĩa từ vựng thất bại");

            // Thành công: trả về 201 Created
            vocabularyDto.VocabularyId = vocabulary.VocabularyId;
            return CreatedAtAction(nameof(GetVocabularyById),
                                   new { id = vocabulary.VocabularyId },
                                   new { message = "Thêm thành công", data = vocabularyDto });
        }


        // Sửa từ vựng
        [HttpPut("UpdateVocabulary/{id}")]
        public async Task<IActionResult> UpdateVocabulary(int id, [FromBody] VocabularyDTO vocabularyDto)
        {
            if (id != vocabularyDto.VocabularyId)
            {
                return BadRequest(new { message = "ID không khớp" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vocabulary = await db.Vocabularies
                .Include(v => v.VocabularyMeanings)
                .FirstOrDefaultAsync(v => v.VocabularyId == id);

            if (vocabulary == null)
            {
                return NotFound(new { message = $"Không tìm thấy từ vựng với ID {id}" });
            }

            vocabulary.Word = vocabularyDto.Word;
            vocabulary.Pronunciation = vocabularyDto.Pronunciation;
            vocabulary.AudioUrlUk = vocabularyDto.AudioUrlUk;
            vocabulary.AudioUrlUs = vocabularyDto.AudioUrlUs;
            // tác dụng             db.VocabularyMeanings.RemoveRange(vocabulary.VocabularyMeanings); là gì 
            // Xóa các nghĩa cũ

            db.VocabularyMeanings.RemoveRange(vocabulary.VocabularyMeanings);

            foreach (var meaningDto in vocabularyDto.Meanings)
            {
                var meaning = new VocabularyMeaning
                {
                    VocabularyId = vocabulary.VocabularyId,
                    Meaning = meaningDto.Meaning,
                    ExampleSentence = meaningDto.ExampleSentence,
                    TranslatedMeaning = meaningDto.TranslatedMeaning,
                    TranslatedExampleSentence = meaningDto.TranslatedExampleSentence,
                    Synonyms = meaningDto.Synonyms,
                    Antonyms = meaningDto.Antonyms,
                    PartOfSpeech = meaningDto.PartOfSpeech
                };
                db.VocabularyMeanings.Add(meaning);
            }

            await db.SaveChangesAsync();
            return NoContent();
        }

        // Xóa từ vựng
        [HttpDelete("DeleteVocabulary/{id}")]
        public async Task<IActionResult> DeleteVocabulary(int id)
        {
            var vocabulary = await db.Vocabularies
                .Include(v => v.VocabularyMeanings)
                .FirstOrDefaultAsync(v => v.VocabularyId == id);
             
            if (vocabulary == null)
            {
                return NotFound(new { message = $"Không tìm thấy từ vựng với ID {id}" });
            }

            db.VocabularyMeanings.RemoveRange(vocabulary.VocabularyMeanings);
            db.Vocabularies.Remove(vocabulary);
            await db.SaveChangesAsync();

            return NoContent();
        }

        //lấy danh sách từ vựng
        [HttpGet("GetListVocabulary")]
        public async Task<IActionResult> GetListVocabulary()
        {
            var vocabularyDTOs = await db.Vocabularies
                .Select(c => new VocabularyDTO
                {
                    VocabularyId = c.VocabularyId,
                    Word = c.Word,
                    Pronunciation = c.Pronunciation,
                    AudioUrlUk = c.AudioUrlUk,
                    AudioUrlUs = c.AudioUrlUs,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(vocabularyDTOs);
        }

        [HttpGet("GetNewVocabulariesThisMonth")]
        public async Task<IActionResult> GetNewVocabulariesThisMonth()
        {
            var now = DateTime.Now;

            var list = await db.Vocabularies
                .Where(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Year == now.Year
                         && v.CreatedAt.Value.Month == now.Month)
                .OrderByDescending(v => v.CreatedAt) // Sắp xếp theo CreatedAt giảm dần (mới nhất trước)
                .Select(v => new VocabularyDTO
                {
                    VocabularyId = v.VocabularyId,
                    Word = v.Word,
                    Pronunciation = v.Pronunciation,
                    AudioUrlUk = v.AudioUrlUk,
                    AudioUrlUs = v.AudioUrlUs,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // Thêm endpoint lấy tất cả nghĩa của từ vựng theo id
        [HttpGet("GetMeaningsByVocabularyId/{vocabularyId}")]
        public async Task<IActionResult> GetMeaningsByVocabularyId(int vocabularyId)
        {
            var meanings = await db.VocabularyMeanings
                .Where(m => m.VocabularyId == vocabularyId)
                .Select(m => new VocabularyMeaningDTO
                {
                    VocabularyMeaningId = m.VocabularyMeaningId,
                    VocabularyId = m.VocabularyId,
                    Meaning = m.Meaning,
                    ExampleSentence = m.ExampleSentence,
                    TranslatedMeaning = m.TranslatedMeaning,
                    TranslatedExampleSentence = m.TranslatedExampleSentence,
                    Synonyms = m.Synonyms,
                    Antonyms = m.Antonyms,
                    PartOfSpeech = m.PartOfSpeech
                })
                .ToListAsync();

            if (!meanings.Any())
            {
                return NotFound(new { message = $"Không tìm thấy nghĩa cho từ vựng với ID {vocabularyId}" });
            }

            return Ok(meanings);
        }

        // 2. Sau đó, thêm endpoint trong controller
        [HttpGet("GetVocabularyWithMeanings/{vocabularyId}")]
        public async Task<IActionResult> GetVocabularyWithMeanings(int vocabularyId)
        {
            var vocab = await db.Vocabularies
                // Lọc theo id truyền vào
                .Where(v => v.VocabularyId == vocabularyId)
                // Project xuống DTO, kèm theo danh sách meanings
                .Select(v => new VocabularyWithMeaningsDTO
                {
                    VocabularyId = v.VocabularyId,
                    Word = v.Word,
                    Pronunciation = v.Pronunciation,
                    AudioUrlUk = v.AudioUrlUk,
                    AudioUrlUs = v.AudioUrlUs,
                    CreatedAt = v.CreatedAt,
                    Meanings = v.VocabularyMeanings
                                        .Select(m => new VocabularyMeaningDTO
                                        {
                                            VocabularyMeaningId = m.VocabularyMeaningId,
                                            VocabularyId = m.VocabularyId,
                                            Meaning = m.Meaning,
                                            ExampleSentence = m.ExampleSentence,
                                            TranslatedMeaning = m.TranslatedMeaning,
                                            TranslatedExampleSentence = m.TranslatedExampleSentence,
                                            Synonyms = m.Synonyms,
                                            Antonyms = m.Antonyms,
                                            PartOfSpeech = m.PartOfSpeech
                                        })
                                        .ToList()
                })
                .FirstOrDefaultAsync();  // Lấy 1 record hoặc null

            if (vocab == null)
            {
                return NotFound(new { message = $"Không tìm thấy từ vựng với ID {vocabularyId}" });
            }

            return Ok(vocab);
        }

        [HttpGet("GetVocabularyById/{id}")]
        public async Task<IActionResult> GetVocabularyById(int id)
        {
            var vocabulary = await db.Vocabularies
                .Include(v => v.VocabularyMeanings)
                .FirstOrDefaultAsync(v => v.VocabularyId == id);
            if (vocabulary == null)
            {
                return NotFound();
            }
            var vocabularyDTO = new VocabularyDTO
            {
                VocabularyId = vocabulary.VocabularyId,
                Word = vocabulary.Word,
                Pronunciation = vocabulary.Pronunciation,
                AudioUrlUk = vocabulary.AudioUrlUk,
                AudioUrlUs = vocabulary.AudioUrlUs,
                CreatedAt = vocabulary.CreatedAt,
                Meanings = vocabulary.VocabularyMeanings.Select(m => new VocabularyMeaningDTO
                {
                    VocabularyMeaningId = m.VocabularyMeaningId,
                    VocabularyId = m.VocabularyId,
                    Meaning = m.Meaning,
                    ExampleSentence = m.ExampleSentence,
                    TranslatedMeaning = m.TranslatedMeaning,
                    TranslatedExampleSentence = m.TranslatedExampleSentence,
                    Synonyms = m.Synonyms,
                    Antonyms = m.Antonyms,
                    PartOfSpeech = m.PartOfSpeech
                }).ToList()
            };
            return Ok(vocabularyDTO);
        }



        //=============VocabularyCategoryMaping=================


        // Lấy danh sách tất cả VocabularyCategoryMapping
        [HttpGet("GetListVocabularyCategoryMapping")]
        public async Task<IActionResult> GetListVocabularyCategoryMapping()
        {
            var mappings = await db.VocabularyCategoryMappings
                .Include(m => m.Vocabulary)
                .Include(m => m.VocabularyCategory)
                .Select(m => new
                {
                    m.VocabularyId,
                    m.VocabularyCategoryId,
                    m.DateAdded
                })
                .ToListAsync();

            return Ok(mappings);
        }
        // Lấy danh sách từ vựng theo VocabularyCategoryId
        // Lấy danh sách VocabularyId và DateAdded theo VocabularyCategoryId
        [HttpGet("GetListVocabularyCategoryMappingByCategoryId/{categoryId}")]
        public async Task<IActionResult> GetListVocabularyCategoryMappingByCategoryId(int categoryId)
        {
            var result = await db.VocabularyCategoryMappings
                .Where(m => m.VocabularyCategoryId == categoryId)
                .Select(m => new
                {
                    m.VocabularyId,
                    m.DateAdded
                })
                .ToListAsync();

            return Ok(result);
        }
        

        // Thêm mapping từ vựng vào category
        [HttpPost("AddVocabularyCategoryMapping")]
        public async Task<IActionResult> AddVocabularyCategoryMapping([FromBody] VocabularyCategoryMappingDTO dto)
        {
            // Kiểm tra tồn tại Vocabulary
            var vocabulary = await db.Vocabularies.FindAsync(dto.VocabularyId);
            if (vocabulary == null)
                return NotFound(new { message = $"Không tìm thấy từ vựng với ID {dto.VocabularyId}" });

            // Kiểm tra tồn tại Category
            var category = await db.VocabularyCategories.FindAsync(dto.VocabularyCategoryId);
            if (category == null)
                return NotFound(new { message = $"Không tìm thấy loại với ID {dto.VocabularyCategoryId}" });

            // Kiểm tra trùng lặp
            var exists = await db.VocabularyCategoryMappings
                .AnyAsync(m => m.VocabularyId == dto.VocabularyId && m.VocabularyCategoryId == dto.VocabularyCategoryId);
            if (exists)
                return Conflict(new { message = "Mapping đã tồn tại." });

            var mapping = new VocabularyCategoryMapping
            {
                VocabularyId = dto.VocabularyId,
                VocabularyCategoryId = dto.VocabularyCategoryId,
                DateAdded = DateTime.Now
            };
            db.VocabularyCategoryMappings.Add(mapping);
            await db.SaveChangesAsync();
            return Ok(new { message = "Thêm thành công", data = dto });
        }

        // Xóa mapping từ vựng khỏi category
        [HttpDelete("DeleteVocabularyCategoryMapping")]
        public async Task<IActionResult> DeleteVocabularyCategoryMapping([FromBody] VocabularyCategoryMappingDTO dto)
        {
            var mapping = await db.VocabularyCategoryMappings
                .FirstOrDefaultAsync(m => m.VocabularyId == dto.VocabularyId && m.VocabularyCategoryId == dto.VocabularyCategoryId);
            if (mapping == null)
                return NotFound(new { message = "Không tìm formthấy mapping cần xóa." });

            db.VocabularyCategoryMappings.Remove(mapping);
            await db.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công" });
        }

        // Sửa mapping (chỉ cho phép đổi category, không đổi vocabulary)
        [HttpPut("UpdateVocabularyCategoryMapping")]
        public async Task<IActionResult> UpdateVocabularyCategoryMapping([FromBody] VocabularyCategoryMappingDTO dto, [FromQuery] int newCategoryId)
        {
            // Kiểm tra tồn tại mapping cũ
            var mapping = await db.VocabularyCategoryMappings
                .FirstOrDefaultAsync(m => m.VocabularyId == dto.VocabularyId && m.VocabularyCategoryId == dto.VocabularyCategoryId);
            if (mapping == null)
                return NotFound(new { message = "Không tìm thấy mapping cần sửa." });

            // Kiểm tra tồn tại category mới
            var newCategory = await db.VocabularyCategories.FindAsync(newCategoryId);
            if (newCategory == null)
                return NotFound(new { message = $"Không tìm thấy loại với ID {newCategoryId}" });

            // Kiểm tra trùng lặp
            var exists = await db.VocabularyCategoryMappings
                .AnyAsync(m => m.VocabularyId == dto.VocabularyId && m.VocabularyCategoryId == newCategoryId);
            if (exists)
                return Conflict(new { message = "Mapping với category mới đã tồn tại." });

            mapping.VocabularyCategoryId = newCategoryId;
            mapping.DateAdded = DateTime.Now;
            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công" });
        }


        //=========VocabularyCategory=================
        // Lấy danh sách tất cả VocabularyCategory
        // Lấy danh sách tất cả VocabularyCategory với số lượng từ vựng mà vocabularyCategoryMapping đã đăng ký 
        [HttpGet("GetListVocabularyCategory")]
        public async Task<IActionResult> GetListVocabularyCategory()
        {
            var categories = await db.VocabularyCategories
                .GroupJoin(
                    db.VocabularyCategoryMappings,
                    category => category.VocabularyCategoryId,
                    mapping => mapping.VocabularyCategoryId,
                    (category, mappings) => new VocabularyCategoryDTO
                    {
                        VocabularyCategoryId = category.VocabularyCategoryId,
                        VocabularyCategoryName = category.VocabularyCategoryName,
                        VocabularyCategoryDescription = category.VocabularyCategoryDescription,
                        UrlImage = category.UrlImage,
                        CreatedAt = category.CreatedAt,
                        VocabularyCount = mappings.Count() // Đếm số lượng từ vựng trong category
                    })
                .ToListAsync();
            return Ok(categories);
        }

        // Thêm mới VocabularyCategory
        [HttpPost("AddVocabularyCategory")]
        public async Task<IActionResult> AddVocabularyCategory([FromBody] VocabularyCategoryDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.VocabularyCategoryName))
                return BadRequest(new { message = "Tên loại không được để trống" });

            var entity = new VocabularyCategory
            {
                VocabularyCategoryName = dto.VocabularyCategoryName,
                VocabularyCategoryDescription = dto.VocabularyCategoryDescription,
                UrlImage = dto.UrlImage,
                CreatedAt = DateTime.Now
            };

            db.VocabularyCategories.Add(entity);
            await db.SaveChangesAsync();

            dto.VocabularyCategoryId = entity.VocabularyCategoryId;
            dto.CreatedAt = entity.CreatedAt;
            return Ok(new { message = "Thêm thành công", data = dto });
        }

        // Sửa VocabularyCategory
        [HttpPut("UpdateVocabularyCategory/{id}")]
        public async Task<IActionResult> UpdateVocabularyCategory(int id, [FromBody] VocabularyCategoryDTO dto)
        {
            var entity = await db.VocabularyCategories.FindAsync(id);
            if (entity == null)
                return NotFound(new { message = $"Không tìm thấy loại với ID {id}" });

            if (string.IsNullOrWhiteSpace(dto.VocabularyCategoryName))
                return BadRequest(new { message = "Tên loại không được để trống" });

            entity.VocabularyCategoryName = dto.VocabularyCategoryName;
            entity.VocabularyCategoryDescription = dto.VocabularyCategoryDescription;
            entity.UrlImage = dto.UrlImage;
            await db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công", data = dto });
        }

        // Xóa VocabularyCategory
        [HttpDelete("DeleteVocabularyCategory/{id}")]
        public async Task<IActionResult> DeleteVocabularyCategory(int id)
        {
            var entity = await db.VocabularyCategories
                .Include(c => c.VocabularyCategoryMappings)
                .FirstOrDefaultAsync(c => c.VocabularyCategoryId == id);

            if (entity == null)
                return NotFound(new { message = $"Không tìm thấy loại với ID {id}" });

            // Xóa các mapping liên quan trước (nếu có)
            if (entity.VocabularyCategoryMappings.Any())
                db.VocabularyCategoryMappings.RemoveRange(entity.VocabularyCategoryMappings);

            db.VocabularyCategories.Remove(entity);
            await db.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công" });
        }

    }
}