// Lấy meanings từ server
var rawJson = document.getElementById("MeaningsJsonServer").textContent.trim();
var meanings = [];
try { meanings = JSON.parse(rawJson); } catch { meanings = []; }

// Hàm render editor
function renderMeaningsEditor(meanings) {
    var container = $("#meaningsEditor");
    container.empty();
    if (meanings.length === 0) {
        container.append('<div class="text-muted">Chưa có nghĩa nào. Nhấn "Thêm nghĩa" để bắt đầu.</div>');
    }
    meanings.forEach(function (m, idx) {
        var html = `
        <div class="card mb-2" data-idx="${idx}">
            <div class="card-body">
                <div class="mb-2">
                    <label>Nghĩa</label>
                    <input type="text" class="form-control meaning-input" value="${m.Meaning || ''}" />
                </div>
                <div class="mb-2">
                    <label>Loại từ</label>
                    <input type="text" class="form-control partofspeech-input" value="${m.PartOfSpeech || ''}" />
                </div>
                <div class="mb-2">
                    <label>Ví dụ</label>
                    <input type="text" class="form-control example-input" value="${m.ExampleSentence || ''}" />
                </div>
                <div class="mb-2">
                    <label>Nghĩa dịch</label>
                    <input type="text" class="form-control translatedmeaning-input" value="${m.TranslatedMeaning || ''}" />
                </div>
                <button type="button" class="btn btn-danger btn-sm remove-meaning">Xóa</button>
            </div>
        </div>`;
        container.append(html);
    });
    container.append('<button type="button" class="btn btn-success btn-sm" id="addMeaning">Thêm nghĩa</button>');
}

// Sự kiện thêm/xóa/sửa
$(document).on("click", "#addMeaning", function () {
    meanings.push({});
    renderMeaningsEditor(meanings);
});
$(document).on("click", ".remove-meaning", function () {
    var idx = $(this).closest(".card").data("idx");
    meanings.splice(idx, 1);
    renderMeaningsEditor(meanings);
});
$(document).on("input", ".meaning-input, .partofspeech-input, .example-input, .translatedmeaning-input", function () {
    var idx = $(this).closest(".card").data("idx");
    meanings[idx].Meaning = $(this).closest(".card").find(".meaning-input").val();
    meanings[idx].PartOfSpeech = $(this).closest(".card").find(".partofspeech-input").val();
    meanings[idx].ExampleSentence = $(this).closest(".card").find(".example-input").val();
    meanings[idx].TranslatedMeaning = $(this).closest(".card").find(".translatedmeaning-input").val();
});

// Khi submit form, serialize meanings vào input ẩn
$("#vocabForm").on("submit", function () {
    $("#MeaningsJson").val(JSON.stringify(meanings));
});
$(document).ready(function () {
    renderMeaningsEditor(meanings);
});
