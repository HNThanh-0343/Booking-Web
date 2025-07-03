document.querySelectorAll('[data-emsp]').forEach(el => {
    const count = parseInt(el.getAttribute('data-emsp'));
    if (!isNaN(count) && count > 0) {
        el.innerHTML = '&emsp;'.repeat(count);
    }
});

function loadPartial(containerId, url) {
    fetch(url)
        .then(res => res.text())
        .then(html => {
            $(`#${containerId}`).empty().append(html);
        })
        .catch(err => {
            placeholder.innerHTML = "<div class='text-danger'>Lỗi tải dữ liệu</div>";
        });
}

document.addEventListener("DOMContentLoaded", function () {
    loadPartial("ChoNghiDaXem", "/TrangChu/ChoNghiDaXem");
    loadPartial("DiemDenHangDaus", "/TrangChu/DiemDenHangDau");
});

