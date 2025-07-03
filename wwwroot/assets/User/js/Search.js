function setupDatePicker(selector) {
    flatpickr(selector, {
        mode: "range",
        dateFormat: "d/m/y",
        minDate: "today",
        locale: "vn",
        onChange: function (selectedDates, dateStr, instance) {
            if (selectedDates.length === 2) {
                const start = selectedDates[0];
                const end = selectedDates[1];

                const format = (d) => {
                    const day = String(d.getDate()).padStart(2, "0");
                    const month = String(d.getMonth() + 1).padStart(2, "0");
                    const year = String(d.getFullYear()).slice(-2);
                    return `${day}/${month}/${year}`;
                };

                instance.input.value = format(start) + " đến " + format(end);
            }
        },
    });
}

function setupRoomGuestSummary() {
    document.addEventListener("DOMContentLoaded", function () {
        function updateRoomGuestSummary() {
            const results = document.querySelectorAll('#basicDropdownClick .js-result');

            if (results.length < 3) return;

            const roomCount = parseInt(results[0].value) || 0;
            const adultCount = parseInt(results[1].value) || 0;
            const childCount = parseInt(results[2].value) || 0;
            const totalGuests = adultCount + childCount;

            const summaryText = `${roomCount} phòng, ${totalGuests} khách`;
            document.getElementById('roomGuestSummary').textContent = summaryText;
        }

        // Gọi ngay khi DOM sẵn sàng
        updateRoomGuestSummary();

        // Gắn sự kiện khi nhấn nút cộng hoặc trừ
        const plusMinusButtons = document.querySelectorAll('#basicDropdownClick .js-plus, #basicDropdownClick .js-minus');
        plusMinusButtons.forEach(button => {
            button.addEventListener("click", function () {
                // Timeout để chờ input thay đổi sau khi xử lý tăng/giảm
                setTimeout(updateRoomGuestSummary, 50);
            });
        });
    });
}

var lastSuggestionsTinhThanh = "";
var count = 0;
function setupLocationAutocomplete(tinhThanhList, hotelList) {
    const suggestionBox = document.getElementById('suggestionBox');
    const allSuggestions = [...tinhThanhList, ...hotelList];
    count = allSuggestions.length;
    function showSuggestions(list) {
        // Nếu đã có HTML cache sẵn thì chỉ cần render lại
        if (list.length == count) {
            suggestionBox.innerHTML = lastSuggestionsTinhThanh;
            suggestionBox.style.display = 'block';
            return;
        }

        let html = `<div class="list-group-item disabled bg-light font-weight-bold text-dark">
                        📍 Một số gợi ý cho bạn
                    </div>`;

        list.forEach(item => {

            const locationText = item.Local ? item.Local : 'Việt Nam';
            html += `
        <a href="#" class="list-group-item list-group-item-action d-flex align-items-start" onclick="event.preventDefault(); locationInput.value='${item.Name}'; suggestionBox.style.display='none';">
            <i class="flaticon-pin-1 mr-2 text-primary font-size-18"></i>
            <div>
                <div class="font-weight-bold">${item.Name}</div>
                <div class="text-muted font-size-12 text-left">${locationText}</div>
            </div>
        </a>`;
        });

        lastSuggestionsTinhThanh = html; // cache lại HTML
        suggestionBox.innerHTML = html;
        suggestionBox.style.display = 'block';
    }

    $('#locationInput').on('input', function () {
        const keyword = this.value.toLowerCase().trim();
        if (keyword === '') {
            suggestionBox.style.display = 'none';
            return;
        }

        
        const filtered = allSuggestions.filter(item => item.Name.toLowerCase().includes(keyword));
        if (filtered.length === 0) {
            suggestionBox.style.display = 'none';
            return;
        }

        showSuggestions(filtered);
    });
    $('#locationInput').on('click', function () {
        if (this.value.trim() === '') {
            showSuggestions(tinhThanhList);
        }
    });
    $('#locationInput').on('blur', function (e) {
        setTimeout(function () {
            suggestionBox.style.display = 'none';
        }, 200); // đủ thời gian để click diễn ra
    });
}

function setupPriceSlider(maxPrice, getDK) {
    const slider = document.getElementById('customRangeSlider');
    const bar = slider.querySelector('.irs-bar');
    const fromSlider = slider.querySelector('.irs-slider.from');
    const toSlider = slider.querySelector('.irs-slider.to');
    const minResult = document.getElementById('rangeSliderExample3MinResult');
    const maxResult = document.getElementById('rangeSliderExample3MaxResult');

    const sliderWidth = 300; // px, bằng với style width của slider

    let dragging = null; // "from" hoặc "to"

    // Giá trị tính theo % của track (0..100)
    let fromPercent = 0;
    let toPercent = 100;

    // Helper format VNĐ
    function formatVND(num) {
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    let filterTimeout;
    function updateUI() {
        if (fromPercent > toPercent) fromPercent = toPercent;
        if (toPercent < fromPercent) toPercent = fromPercent;

        fromSlider.style.left = fromPercent + '%';
        toSlider.style.left = toPercent + '%';

        bar.style.left = fromPercent + '%';
        bar.style.width = (toPercent - fromPercent) + '%';

        let fromVal = Math.round(fromPercent / 100 * maxPrice);
        let toVal = Math.round(toPercent / 100 * maxPrice);

        minResult.textContent = formatVND(fromVal);
        maxResult.textContent = formatVND(toVal);

        clearTimeout(filterTimeout);
        filterTimeout = setTimeout(() => {
            if (typeof getDK === "function") getDK();
        }, 300);
    }

    function onDragStart(e) {
        e.preventDefault();
        if (e.target === fromSlider) {
            dragging = "from";
        } else if (e.target === toSlider) {
            dragging = "to";
        }
    }

    function onDragEnd(e) {
        dragging = null;
    }

    function onDragMove(e) {
        if (!dragging) return;

        let clientX = e.clientX !== undefined ? e.clientX : e.touches[0].clientX;
        let rect = slider.getBoundingClientRect();
        let x = clientX - rect.left;

        if (x < 0) x = 0;
        if (x > sliderWidth) x = sliderWidth;

        let percent = (x / sliderWidth) * 100;

        if (dragging === "from") {
            fromPercent = Math.min(percent, toPercent);
        } else if (dragging === "to") {
            toPercent = Math.max(percent, fromPercent);
        }

        updateUI();
    }

    fromSlider.addEventListener('mousedown', onDragStart);
    toSlider.addEventListener('mousedown', onDragStart);

    window.addEventListener('mouseup', onDragEnd);
    window.addEventListener('mousemove', onDragMove);

    // Touch support
    fromSlider.addEventListener('touchstart', onDragStart);
    toSlider.addEventListener('touchstart', onDragStart);

    window.addEventListener('touchend', onDragEnd);
    window.addEventListener('touchmove', onDragMove);

    updateUI();
}

function setupSidebarToggle() {
    const toggleBtn = document.getElementById('toggleSidebar');
    const sidebar = document.getElementById('sidebar');
    const arrowIcon = document.getElementById('arrowIcon');

    if (!toggleBtn || !sidebar || !arrowIcon) {
        console.warn('Thiếu phần tử HTML cho sidebar toggle');
        return;
    }

    toggleBtn.addEventListener('click', function () {
        const isShown = sidebar.classList.contains('show');

        if (isShown) {
            sidebar.classList.remove('show');
            arrowIcon.classList.remove('fa-caret-square-up');
            arrowIcon.classList.add('fa-caret-square-down');
        } else {
            sidebar.classList.add('show');
            arrowIcon.classList.remove('fa-caret-square-down');
            arrowIcon.classList.add('fa-caret-square-up');
        }
    });
}

function toggleSort(key, allKeys, getDK) {
    let existing = sortOrder.find(s => s.key === key);

    if (!existing) {
        // Nếu chưa có, thêm mới với tăng dần
        sortOrder.push({ key: key, direction: "asc" });
    } else {
        // Luân phiên giữa asc <-> desc
        existing.direction = existing.direction === "asc" ? "desc" : "asc";
    }

    updateSortIcons(allKeys);

    // Gọi hàm getDK nếu nó là hàm
    if (typeof getDK === 'function') {
        getDK();
    }
}

function updateSortIcons(allKeys) {
    const icons = {
        default: '<i class="fas fa-sort ml-1"></i>',      // Cả 2 mũi tên
        asc: '<i class="fas fa-sort-up ml-1"></i>',       // Mũi tên lên
        desc: '<i class="fas fa-sort-down ml-1"></i>'     // Mũi tên xuống
    };

    // Reset tất cả nút về icon mặc định
    allKeys.forEach(k => {
        $(`#btn${k} span`).html(icons.default);
    });

    // Cập nhật icon theo thứ tự trong sortOrder
    sortOrder.forEach(sort => {
        $(`#btn${sort.key} span`).html(icons[sort.direction]);
    });
}

function setupGuestCountInput(inputElement) {
    if (!inputElement) return;

    inputElement.addEventListener('input', function () {
        let value = this.value.replace(/[^0-9]/g, '');

        if (value === '' || isNaN(parseInt(value))) {
            value = '1';
        }

        let numValue = parseInt(value);
        if (numValue < 1) {
            value = '1';
        } else if (numValue > 999) {
            value = '999';
        }

        this.value = value;
    });

    inputElement.addEventListener('blur', function () {
        if (this.value === '' || isNaN(parseInt(this.value))) {
            this.value = '1';
        }
    });
}

function replaceWithPlaceholder(img, type = "") {
    const picture = img.parentNode;
    const placeholder = document.createElement("div");


    if (type == "hotel") {
        placeholder.className = "IconBed";
        placeholder.innerHTML = `
		<span class="IconBed" aria-hidden="true">
		  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="50px">
			<path d="M2.75 12h18.5c.69 0 1.25.56 1.25 1.25V18l.75-.75H.75l.75.75v-4.75c0-.69.56-1.25 1.25-1.25m0-1.5A2.75 2.75 0 0 0 0 13.25V18c0 .414.336.75.75.75h22.5A.75.75 0 0 0 24 18v-4.75a2.75 2.75 0 0 0-2.75-2.75zM0 18v3a.75.75 0 0 0 1.5 0v-3A.75.75 0 0 0 0 18m22.5 0v3a.75.75 0 0 0 1.5 0v-3a.75.75 0 0 0-1.5 0m-.75-6.75V4.5a2.25 2.25 0 0 0-2.25-2.25h-15A2.25 2.25 0 0 0 2.25 4.5v6.75a.75.75 0 0 0 1.5 0V4.5a.75.75 0 0 1 .75-.75h15a.75.75 0 0 1 .75.75v6.75a.75.75 0 0 0 1.5 0m-13.25-3h7a.25.25 0 0 1 .25.25v2.75l.75-.75h-9l.75.75V8.5a.25.25 0 0 1 .25-.25m0-1.5A1.75 1.75 0 0 0 6.75 8.5v2.75c0 .414.336.75.75.75h9a.75.75 0 0 0 .75-.75V8.5a1.75 1.75 0 0 0-1.75-1.75z"></path>
		  </svg>
		</span>
	  `;
    } else if (type == "Blog") {
        placeholder.className = "IconBog";
        placeholder.innerHTML = `
		<span class="IconBog" aria-hidden="true">
		  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="50px">
			<path d="M22.5 12v9.75a.75.75 0 0 1-.75.75H2.25a.75.75 0 0 1-.75-.75V2.25a.75.75 0 0 1 .75-.75h19.5a.75.75 0 0 1 .75.75zm1.5 0V2.25A2.25 2.25 0 0 0 21.75 0H2.25A2.25 2.25 0 0 0 0 2.25v19.5A2.25 2.25 0 0 0 2.25 24h19.5A2.25 2.25 0 0 0 24 21.75zM5.85 17.7l3.462-4.616a.75.75 0 0 1 1.13-.08l1.028 1.026a.75.75 0 0 0 1.13-.08l3.3-4.4a.75.75 0 0 1 1.2 0l2.67 3.56a.75.75 0 1 0 1.2-.9L18.3 8.65a2.248 2.248 0 0 0-3.6 0l-3.3 4.4 1.13-.08-1.027-1.027a2.25 2.25 0 0 0-3.391.242L4.65 16.8a.75.75 0 1 0 1.2.9M7.5 6.375a1.125 1.125 0 1 1-2.25 0 1.125 1.125 0 0 1 2.25 0m1.5 0a2.625 2.625 0 1 0-5.25 0 2.625 2.625 0 0 0 5.25 0M.75 18h22.5a.75.75 0 0 0 0-1.5H.75a.75.75 0 0 0 0 1.5"></path>
		  </svg>
		</span>
	  `;
    } else if (type == "KM") {
        placeholder.className = "IconKM";
        placeholder.innerHTML = `
		<span class="IconKM" aria-hidden="true">
		  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="50px">
			<path d="M22.5 12v9.75a.75.75 0 0 1-.75.75H2.25a.75.75 0 0 1-.75-.75V2.25a.75.75 0 0 1 .75-.75h19.5a.75.75 0 0 1 .75.75zm1.5 0V2.25A2.25 2.25 0 0 0 21.75 0H2.25A2.25 2.25 0 0 0 0 2.25v19.5A2.25 2.25 0 0 0 2.25 24h19.5A2.25 2.25 0 0 0 24 21.75zM5.85 17.7l3.462-4.616a.75.75 0 0 1 1.13-.08l1.028 1.026a.75.75 0 0 0 1.13-.08l3.3-4.4a.75.75 0 0 1 1.2 0l2.67 3.56a.75.75 0 1 0 1.2-.9L18.3 8.65a2.248 2.248 0 0 0-3.6 0l-3.3 4.4 1.13-.08-1.027-1.027a2.25 2.25 0 0 0-3.391.242L4.65 16.8a.75.75 0 1 0 1.2.9M7.5 6.375a1.125 1.125 0 1 1-2.25 0 1.125 1.125 0 0 1 2.25 0m1.5 0a2.625 2.625 0 1 0-5.25 0 2.625 2.625 0 0 0 5.25 0M.75 18h22.5a.75.75 0 0 0 0-1.5H.75a.75.75 0 0 0 0 1.5"></path>
		  </svg>
		</span>
	  `;
    } else if (type == "BlogChiTiet") {
        placeholder.className = "IconBogChiTiet";
        placeholder.innerHTML = `
		<span class="IconBogChiTiet" aria-hidden="true">
		  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="50px">
			<path d="M22.5 12v9.75a.75.75 0 0 1-.75.75H2.25a.75.75 0 0 1-.75-.75V2.25a.75.75 0 0 1 .75-.75h19.5a.75.75 0 0 1 .75.75zm1.5 0V2.25A2.25 2.25 0 0 0 21.75 0H2.25A2.25 2.25 0 0 0 0 2.25v19.5A2.25 2.25 0 0 0 2.25 24h19.5A2.25 2.25 0 0 0 24 21.75zM5.85 17.7l3.462-4.616a.75.75 0 0 1 1.13-.08l1.028 1.026a.75.75 0 0 0 1.13-.08l3.3-4.4a.75.75 0 0 1 1.2 0l2.67 3.56a.75.75 0 1 0 1.2-.9L18.3 8.65a2.248 2.248 0 0 0-3.6 0l-3.3 4.4 1.13-.08-1.027-1.027a2.25 2.25 0 0 0-3.391.242L4.65 16.8a.75.75 0 1 0 1.2.9M7.5 6.375a1.125 1.125 0 1 1-2.25 0 1.125 1.125 0 0 1 2.25 0m1.5 0a2.625 2.625 0 1 0-5.25 0 2.625 2.625 0 0 0 5.25 0M.75 18h22.5a.75.75 0 0 0 0-1.5H.75a.75.75 0 0 0 0 1.5"></path>
		  </svg>
		</span>
	  `;
    } else if (type == "in") {
        placeholder.className = "IconIn";
        placeholder.innerHTML = `
		<span class="IconIn" aria-hidden="true">
		  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="50px">
			<path d="M22.5 12v9.75a.75.75 0 0 1-.75.75H2.25a.75.75 0 0 1-.75-.75V2.25a.75.75 0 0 1 .75-.75h19.5a.75.75 0 0 1 .75.75zm1.5 0V2.25A2.25 2.25 0 0 0 21.75 0H2.25A2.25 2.25 0 0 0 0 2.25v19.5A2.25 2.25 0 0 0 2.25 24h19.5A2.25 2.25 0 0 0 24 21.75zM5.85 17.7l3.462-4.616a.75.75 0 0 1 1.13-.08l1.028 1.026a.75.75 0 0 0 1.13-.08l3.3-4.4a.75.75 0 0 1 1.2 0l2.67 3.56a.75.75 0 1 0 1.2-.9L18.3 8.65a2.248 2.248 0 0 0-3.6 0l-3.3 4.4 1.13-.08-1.027-1.027a2.25 2.25 0 0 0-3.391.242L4.65 16.8a.75.75 0 1 0 1.2.9M7.5 6.375a1.125 1.125 0 1 1-2.25 0 1.125 1.125 0 0 1 2.25 0m1.5 0a2.625 2.625 0 1 0-5.25 0 2.625 2.625 0 0 0 5.25 0M.75 18h22.5a.75.75 0 0 0 0-1.5H.75a.75.75 0 0 0 0 1.5"></path>
		  </svg>
		</span>
	  `;
    }
    picture.replaceChild(placeholder, img);
}


function handleLikeButtonClick(updateUrl, hotelID, btn) {
    var like = btn.data("liked") === true || btn.data("liked") === "true" || btn.data("liked") === "True";
    $.ajax({
        url: updateUrl,
        type: 'POST',
        data: { hotelId: hotelID, liked: !like },
        success: function (response) {
            if (response.success) {
                btn.data("liked", (!like).toString());

                if (!like) {
                    btn.html('<span class="fa-solid fa-heart text-danger"></span>');
                } else {
                    btn.html('<span class="fa-regular fa-heart text-dark"></span>');
                }                                                                                                                                                                                                                           
            } else {
                alert("Có lỗi xảy ra khi cập nhật trạng thái yêu thích!");
            }
        },
        error: function () {
            alert("Lỗi khi gọi server!");
        }
    });
}