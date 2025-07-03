// ngày checkin checkout trong 1 input
flatpickr("#checkInOut", {
    mode: "range", // chọn khoảng ngày
    dateFormat: "d/m/Y", // định dạng ngày
    //defaultDate: [new Date()], // ngày mặc định nếu cần
    locale: "vn", // nếu bạn có file ngôn ngữ tiếng Việt
    showMonths: 1,
    allowInput: false,
    minDate: "today",
});

// giá
var timkiemtheogia = false;

$(function () {
    var urlParams = new URLSearchParams(window.location.search);
    $("#slider-range").slider({
        range: true,
        min: priceMin,
        max: priceMax,
        step: 50000,
        
        values: [(urlParams.has('GiaTu')) ? parseInt(urlParams.get("GiaTu"), 10) : priceMin, (urlParams.has('GiaDen')) ? parseInt(urlParams.get("GiaDen"), 10) : priceMax],
        slide: function (event, ui) {
            $("#amount-min").text(ui.values[0].toLocaleString('en-US'));
            $("#amount-max").text(ui.values[1].toLocaleString('en-US'));
            if (!timkiemtheogia) {
                timkiemtheogia = true;
            }
        },
        change: function () {
            hotel.btnSearch();
        }
    });
    var valMinInit = hotel.roundToNearest500($("#slider-range").slider("values", 0));
    var valMaxInit = hotel.roundToNearest500($("#slider-range").slider("values", 1));
    $("#amount-min").text(valMinInit.toLocaleString('en-US'));
    $("#amount-max").text(valMaxInit.toLocaleString('en-US'));
   
});

var hotel = {
    btnSearch: function () {
        // get ngày đi ngày về
        var getValueDatTime = $('#checkInOut').val(); // Lấy giá trị từ input        
        var [startStr, endStr] = getValueDatTime?.split(" đến ") || [];
        // get sao
        var selectedStars = $('.filter-rating:checked').map(function () {
            return $(this).data('stars');
        }).get().join(',');
        // get thành phố
        var selectedCountry = $('.country-checkbox:checked').map(function () {
            return $(this).data('id');
        }).get().join(',');

        // giá từ
        
        var giatu = $("#amount-min").text(); // ví dụ: "10,000"
        var formatgiatu = parseInt(giatu.replace(/,/g, ''), 10); // kết quả: 10000

        //giá đến
        var giaden = $("#amount-max").text(); // ví dụ: "10,000"
        var formatgiaden = parseInt(giaden.replace(/,/g, ''), 10); // kết quả: 10000
        // get sắp xếp
        var SortKS = $('#ChangeSortKS').val();
        loadpage.reload('#loadFilter', 'Đang tải dữ liệu Khách sạn...');
       
        var data = {
            ksdc: $('#locationInput').val(),// get địa chỉ, tên ks
            ngayden: hotel.toISOFormatDateTime(startStr),// ngày đi 
            ngaydi: hotel.toISOFormatDateTime(endStr),// ngày về
            phong: $('input[name="rooms"]').val(),
            nguoilon: $('input[name="adults"]').val(),
            connit: $('input[name="children"]').val(),
            sao: selectedStars,//get sao
            sort: SortKS,// get sắp xếp
            thanhpho: selectedCountry,// get sắp xếp           
        };
        if (timkiemtheogia) {
            data.giatu = formatgiatu;// get gia tu
            data.giaden = formatgiaden;// get gia den
        }

        const url = new URL(window.location.origin + "/khach-san");
        if ($('#locationInput').val()) url.searchParams.set("location", $('#locationInput').val());
        if (getValueDatTime) url.searchParams.set("dateRange", getValueDatTime);
        if ($('input[name="rooms"]').val()) url.searchParams.set("rooms", $('input[name="rooms"]').val());
        if ($('input[name="adults"]').val()) url.searchParams.set("adults", $('input[name="adults"]').val());
        if ($('input[name="children"]').val()) url.searchParams.set("children", $('input[name="children"]').val());
        if (formatgiatu) url.searchParams.set("GiaTu", formatgiatu);
        if (formatgiaden) url.searchParams.set("GiaDen", formatgiaden);
        if (selectedStars) url.searchParams.set("start", selectedStars);
        if (selectedCountry) url.searchParams.set("country", selectedCountry);
        if (SortKS) url.searchParams.set("sort", SortKS);
        // Cập nhật URL trong trình duyệt
        history.pushState({}, '', url.toString());
        $('#loadFilter').load("/KhachSan/loadFilter", data, function (response, status, xhr) {
            if (status === "success") {
                console.log("Load filter thành công");
            } else {
                console.error("Lỗi khi load filter:", xhr);
            }
        });

        if (window.innerWidth <= 768) {
            document.getElementById("loadFilter").scrollIntoView({
                behavior: "smooth",
                block: "start"
            });
        }
    },
    toISOFormatDateTime: function (dateStr) {
        if (!dateStr || dateStr.length === 0) return;

        var [d, m, y] = dateStr.split('/');
        return `${y}-${m.padStart(2, '0')}-${d.padStart(2, '0')}`;
    },
    roundToNearest500: function (value) {
        return Math.round(value / 50000) * 50000;
    }
};
$(document).on('change', '#ChangeSortKS', function () {
    searchMobile.toggle();
    hotel.btnSearch();
});


$('#shopRatingOne').on('change', '.filter-rating', function () {
    setTimeout(function () {
        searchMobile.toggle();
        hotel.btnSearch();
    }, 200); // đủ thời gian để click diễn ra
});
$('#cityCategoryOne').on('change', '.country-checkbox', function () {
    setTimeout(function () {
        searchMobile.toggle();
        hotel.btnSearch();
    }, 200); // đủ thời gian để click diễn ra
});

var searchMobile = {
    toggle: function () {
        if (window.innerWidth < 992) {
            $('#sidebar').toggleClass('show');
        }

    }
}
