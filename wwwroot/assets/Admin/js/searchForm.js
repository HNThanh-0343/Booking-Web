//Tìm kiếm
$(function () {
    $('#searchValue').on('submit', function () {
        loadData();
    });
});

// Load Data function
function loadData(ActionName) {
    var model = $('#DataTable');

    var path = window.location.pathname;
    var actionSegment = path.split('/').pop();
    var basePath = "";
    if (typeof ActionName === 'undefined') {
        // Kiểm tra actionSegment có bằng Index không
        if (actionSegment.startsWith("Index")) { // Trường hợp controller có nhiều action Index
            var suffix = actionSegment.substring("Index".length);
            ActionName = "ChildIndex" + suffix;
            basePath = path.substring(0, path.lastIndexOf('/'));
        } else {
            ActionName = 'ChildIndex';
            basePath = path;
        }
    }

    var url = basePath + `/${ActionName}`;
    $.ajax({
        url: url,
        data: { searchValue: $.trim($("#searchValue").val()) },
        type: 'GET',
        success: function (result) {
            model.empty().append(result);
        },
        error: function () {
            alert('Không nhận được dữ liệu');
        }
    });
}
$(document).on('click', '#DataTable .pagination li a[href]', function (e) {
    e.preventDefault();
    $.ajax({
        url: $(this).attr("href"),
        data: { searchValue: $.trim($("#searchValue").val()) },
        type: 'GET',
        success: function (result) {
            $('#DataTable').empty().append(result);
        },
        error: function () {
            alert('Không nhận được dữ liệu');
        }
    });
});

// event btn search ()
$('.searchform').on('click', function () {
    var model = $('#DataTable');
    var data = $('.formsearch').find(':input').serialize();
    $.ajax({
        type: "GET",
        url: $(this).data('url'),
        data: data,
        success: function (response) {
            $('#DataTable').empty().append(response);
        },
        error: function (xhr) {
            alert('Không tìm thấy dữ liệu');
        }
    });
});

$('.searchformDateTime').on('click', function () {
    var $form = $('.formsearch');

    // Kiểm tra và chuyển định dạng các input dạng date trước khi serialize
    $form.find(':input').each(function () {
        var name = $(this).attr('name');
        if (name && (name.toLowerCase().includes('date'))) {
            var val = $(this).val();

            // Giả sử val đang ở dạng dd/MM/yyyy HH:mm hoặc dd/MM/yyyy H:mm
            if (val) {
                var formatted = formatToISO(val);
                if (formatted) {
                    $(this).val(formatted);
                }
            }
        }
    });

    var data = $form.find(':input').serialize();

    $.ajax({
        type: "GET",
        url: $(this).data('url'),
        data: data,
        success: function (response) {
            $('#DataTable').empty().append(response);
        },
        error: function (xhr) {
            alert('Không tìm thấy dữ liệu');
        }
    });
});

// Hàm chuyển định dạng dd/MM/yyyy HH:mm => yyyy-MM-ddTHH:mm
function formatToISO(dateStr) {
    // Nếu null hoặc rỗng trả về luôn
    if (!dateStr) return '';

    // Tách ngày giờ
    var parts = dateStr.split(' ');
    if (parts.length < 2) return '';

    var dateParts = parts[0].split('/');
    if (dateParts.length < 3) return '';

    var day = dateParts[0].padStart(2, '0');
    var month = dateParts[1].padStart(2, '0');
    var year = dateParts[2];

    var timePart = parts[1];
    // nếu giờ chỉ 1 số thì padStart(2, '0') nếu cần
    var timeParts = timePart.split(':');
    if (timeParts.length < 2) return '';

    var hour = timeParts[0].padStart(2, '0');
    var minute = timeParts[1].padStart(2, '0');

    return `${year}-${month}-${day}T${hour}:${minute}`;
}

$(document).on('click', '.pagenoSearch.pagination li a[href]', function (e) {
    e.preventDefault();
    e.stopImmediatePropagation();
    var idLoad = $(this).closest('.container').find('.loadUsingPage').attr('id');
    const page = $(this).text().trim(); // hoặc lấy từ data-page
    dataToSend.page = parseInt(page);
    $.ajax({
        url: $(this).attr("href"),
        contentType: "application/json",
        data: JSON.stringify(dataToSend),
        type: 'POST',
        success: function (result) {
            $(`#${idLoad}`).empty().append(result);
        },
        error: function () {
            alert('Không nhận được dữ liệu');
        }
    });
});

$(document).on('click', '.pagination li a[href]', function (e) {
    e.preventDefault();   
    var idLoad = $(this).closest('.container, .container-fluid').find('.loadUsingPage').attr('id');
    var clickedUrl = new URL($(this).attr("href"), window.location.origin);
    var newPage = clickedUrl.searchParams.get("page");

    var currentUrl = new URL(window.location.href);
    var searchParams = currentUrl.searchParams;

    // Cập nhật chỉ page, giữ nguyên các tham số khác
    searchParams.set("page", newPage);

    // Cập nhật URL trên trình duyệt đầy đủ params
    history.pushState(null, '', currentUrl.pathname + '?' + searchParams.toString());
    $.ajax({
        url: '/KhachSan/loadFilter?' + searchParams.toString(),
        data: { searchValue: $.trim($("#searchValue").val()) },
        type: 'GET',
        success: function (result) {
            $(`#${idLoad}`).empty().append(result);
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        },
        error: function () {
            alert('Không nhận được dữ liệu');
        }
    });
});

