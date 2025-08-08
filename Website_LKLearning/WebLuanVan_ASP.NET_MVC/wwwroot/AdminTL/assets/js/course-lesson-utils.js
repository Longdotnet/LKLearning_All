const CourseLessonUtils = {
    initSelect2: function (selector) {
        $(selector).select2({
            theme: 'bootstrap-5',
            placeholder: 'Tìm kiếm...',
            allowClear: true,
            width: '100%'
        });
    },
    bindCheckboxEvents: function (selectAllSelector, rowCheckboxSelector, deleteButtonSelector) {
        $(selectAllSelector).change(function () {
            $(rowCheckboxSelector).prop('checked', $(this).prop('checked'));
            CourseLessonUtils.toggleDeleteButton(rowCheckboxSelector, deleteButtonSelector);
        });
        $(rowCheckboxSelector).change(function () {
            if ($(rowCheckboxSelector + ':checked').length === $(rowCheckboxSelector).length) {
                $(selectAllSelector).prop('checked', true);
            } else {
                $(selectAllSelector).prop('checked', false);
            }
            CourseLessonUtils.toggleDeleteButton(rowCheckboxSelector, deleteButtonSelector);
        });
    },
    toggleDeleteButton: function (rowCheckboxSelector, deleteButtonSelector) {
        var checkedCount = $(rowCheckboxSelector + ':checked').length;
        $(deleteButtonSelector).prop('disabled', checkedCount === 0);
        $(deleteButtonSelector).text(checkedCount > 0 ? `Xóa (${checkedCount}) đã chọn` : 'Xóa đã chọn');
    },
    updateOrder: function (courseId, lessonId, newOrderNo, oldOrderNo, inputElement, totalLessons, updateUrl) {
        if (isNaN(newOrderNo) || newOrderNo < 1 || newOrderNo > totalLessons) {
            toastr.error(`Thứ tự phải từ 1 đến ${totalLessons}`);
            $(inputElement).val(oldOrderNo);
            return;
        }
        if (newOrderNo === oldOrderNo) return;
        $.ajax({
            url: updateUrl,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ courseId, lessonId, orderNo: newOrderNo }),
            beforeSend: function () {
                $(inputElement).prop('disabled', true);
                toastr.info('Đang lưu...');
            },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    CourseLessonUtils.updateUIAfterOrderChange(courseId, lessonId, newOrderNo, oldOrderNo, inputElement);
                } else {
                    toastr.error(response.message || 'Không thể cập nhật thứ tự.');
                    $(inputElement).val(oldOrderNo);
                }
            },
            error: function (xhr) {
                toastr.error('Lỗi khi cập nhật: ' + (xhr.responseJSON?.message || 'Lỗi không xác định'));
                $(inputElement).val(oldOrderNo);
            },
            complete: function () {
                $(inputElement).prop('disabled', false);
            }
        });
    },
    updateUIAfterOrderChange: function (courseId, lessonId, newOrderNo, oldOrderNo, inputElement) {
        // Tương tự hàm hiện tại, nhưng có thể truyền thêm URL để lấy danh sách bài học
    },
    swapLessonOrder: function (courseId, sourceOrder, targetOrder, row, swapUrl) {
        var swapUrl = row.find('.order-input').data('swap-url');
        $.ajax({
            url: swapUrl,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ courseId, sourceOrderNo: sourceOrder, targetOrderNo: targetOrder }),
            success: function (response) {
                if (response.success) {
                    CourseLessonUtils.updateUIAfterSwap(sourceOrder, targetOrder, row);
                } else {
                    toastr.error(response.message || 'Không thể hoán đổi thứ tự bài học.');
                }
            },
            error: function (xhr) {
                toastr.error('Lỗi khi hoán đổi: ' + (xhr.responseJSON?.message || 'Lỗi không xác định'));
            }
        });
    },
    updateUIAfterSwap: function (sourceOrder, targetOrder, row) {
        // Tương tự hàm hiện tại
    },
    debounce: function (func, wait) {
        let timeout;
        return function (...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }
};