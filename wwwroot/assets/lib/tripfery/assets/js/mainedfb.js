jQuery(document).ready(function ($) {
    "use strict";
    $('a[href=\\#]').on('click', function (e) {
        e.preventDefault();
    })

    $('#myTab a').on('click', function (e) {
	  e.preventDefault()
	  $(this).tab('show')
	})

	/* Page scroll Bottom To Top */
    if ($(".scroll-wrap").length) {
        var progressPath = document.querySelector('.scroll-wrap path');
        var pathLength = progressPath.getTotalLength();
        progressPath.style.transition = progressPath.style.WebkitTransition = 'none';
        progressPath.style.strokeDasharray = pathLength + ' ' + pathLength;
        progressPath.style.strokeDashoffset = pathLength;
        progressPath.getBoundingClientRect();
        progressPath.style.transition = progressPath.style.WebkitTransition = 'stroke-dashoffset 10ms linear';
        var updateProgress = function() {
            var scroll = $(window).scrollTop();
            var height = $(document).height() - $(window).height();
            var progress = pathLength - (scroll * pathLength / height);
            progressPath.style.strokeDashoffset = progress;
        }
        updateProgress();
        $(window).scroll(updateProgress);
        var offset = 50;
        var duration = 10;
        jQuery(window).on('scroll', function() {
            if (jQuery(this).scrollTop() > offset) {
                jQuery('.scroll-wrap').addClass('active-scroll');
            } else {
                jQuery('.scroll-wrap').removeClass('active-scroll');
            }
        });
        jQuery('.scroll-wrap').on('click', function(event) {
            event.preventDefault();
            jQuery('html, body').animate({
                scrollTop: 0
            }, duration);
            return false;
        })
    }
    if ($("#checkout_form_block").length) {
        jQuery('#checkout_form_block > *:eq(2), #checkout_form_block > *:eq(3), #checkout_form_block > *:eq(4)').wrapAll('<div class="rt-checkout-content"></div>');
        jQuery('#checkout_form_block > *:eq(0), #checkout_form_block > *:eq(1)').wrapAll('<div class="rt-checkout-form"></div>');
    }

    /*-------------------------------------
    Card
    -------------------------------------*/
    if ($(".rt-booking-layout-1").length) {
        const panels = document.querySelectorAll(".panel");
        window.onload = function () {
            panels[0].classList.add("active");
        };
        panels.forEach((panel) => {
            panel.addEventListener("mouseover", () => {
                removeActiveClasses();
                panel.classList.add("active");
            });
        });
        function removeActiveClasses() {
            panels.forEach((panel) => {
                panel.classList.remove("active");
            });
        }
    }

    /*-------------------------------------
        Toggle Map
    -------------------------------------*/
    if ($(".toggle_map").length) {
        $("#btn_map").on("click", function () {
            $(".toggle_map").toggleClass("rt_show_mag", 1000);
        });
    }
    if ($(".rt-search-customize #search_form .input-group .parent-inner>div:not(.submit)").length) {
        $(".rt-search-customize #search_form .input-group .parent-inner>div:not(.submit)").on("click", function () {
            $(".rt-search-customize #search_form .input-group .parent-inner").toggleClass("rt_show_mag", 1000);
        });
    }

    if ($(".rt_grid_btn").length){
        $(".rt_grid_btn").on("click", function(){
            $(".rt-fillter-inner").removeClass("rt_list_service");
            $(".rt-fillter-inner").addClass("rt_grid_service");
        });
        $(".rt_list_btn").on("click", function () {
            $(".rt-fillter-inner").removeClass("rt_grid_service");
            $(".rt-fillter-inner").addClass("rt_list_service");
        });
    }

    if ($(".rt-search-customize").length) {
        var $searchCustomize = $(".rt-search-customize");
        function handleTabClick(tabSlug) {
            $searchCustomize.removeClass(function (index, className) {
                return (className.match(/\btab-\S+/g) || []).join(' ');
            });
            $searchCustomize.addClass("tab-" + tabSlug);
        }
        $("#search_form_tabs .search_form_tab").on("click", function () {
            var tabSlug = $(this).data("tab-slug");
            console.log({ tabSlug });
            handleTabClick(tabSlug);
        });
    }

    if ($(".rt-share-service").length) {
        $(".rt-share-service").on("click", function(){
            $(".rt-share-service .share-links").fadeToggle();
        });
    }

    

    /*-------------------------------------
    Destination Slider
    -------------------------------------*/
    if ($(".rt-destionation-slider").length) {
        let destionationSlider = new Swiper(".rt-destionation-slider", {
            loop: true,
            slidesPerView: 5,
            grabCursor: true,
            speed: 1000,
            spaceBetween: 30,
            navigation: {
                nextEl: ".topDestinationNav .swiper-button-next",
                prevEl: ".topDestinationNav .swiper-button-prev",
            },
            breakpoints: {
                0: {
                    slidesPerView: 1,
                },
                576: {
                    slidesPerView: 2,
                },
                768: {
                    slidesPerView: 2,
                },
                991: {
                    slidesPerView: 3,
                    spaceBetween: 30,
                },
                1200: {
                    slidesPerView: 5,
                },
            },
        });
    }

    /*-------------------------------------
    Filter Button For Hotel
    -------------------------------------*/
    if ($(".hotel-filter-btns").length) {
        $(".hotel-filter-btns").on("click", "button", function () {
            $(".hotel-filter-btns button").removeClass("active");
            $(this).addClass("active");
            var filterValue = $(this).attr("data-filter");
            $hotelGrid.isotope({ filter: filterValue });
        });

        var $hotelGrid = $(".hotelCardContainer").imagesLoaded(function () {
            // init Isotope after all images have loaded
            $hotelGrid.isotope({
                // options...
                itemSelector: ".card-item",
                layoutMode: "fitRows",
                // filter: "*",
            });
        });
    }

    /*-------------------------------------
        Listing Filter
        -------------------------------------*/
    if ($(".rt-case-isotope").length) {
        $(".listing-filter-btns, .listing-filter-btn").children().first().addClass("active");
        var $grid = $(".cardContainer").imagesLoaded(function () {
            var filterBtnWrapper = $('.listing-filter-btns, .listing-filter-btn');
            if (filterBtnWrapper.length > 0) {
                filterBtnWrapper.each(function () {
                    var firstFilterBtn = $(this).find('.filter-btn:first');
                    var target = $(firstFilterBtn).attr('data-filter');
                    $grid.isotope({
                        itemSelector: ".card-item",
                        layoutMode: "fitRows",
                        filter: target,
                    });
                });
            }
        });

        /*-------------------------------------
        Filter Button
        -------------------------------------*/
        $(".listing-filter-btns, .listing-filter-btn").on("click", "button", function () {
            $(".listing-filter-btns button, .listing-filter-btn button").removeClass("active");
            $(this).addClass("active");
            var filterValue = $(this).attr("data-filter");
            $grid.isotope({ filter: filterValue });
        });
    }

    //isotop track
    if ($('.listing-filter-btns').length) {
        function trackBehavior(tabContainer2, target2 = null) {
            var selectedTrack2 = target2 ? $(target2).parent().find(
                '.filter-btn.active'
            ) : tabContainer2.find('.filter-btn:first');
            var trackPosition2 = $(selectedTrack2).position();
            tabContainer2.find('.rt-color-track').css({
                left: trackPosition2.left + 'px',
                width: selectedTrack2.outerWidth() + 'px',
            });
        }
        var tabContainer2 = $('.listing-filter-btns');
        console.log(tabContainer2);
        trackBehavior(tabContainer2);
        tabContainer2.on('click', '.filter-btn', function (e) {
            var target2 = e.currentTarget;
            trackBehavior(tabContainer2, target2);
        });
    }

    /*-------------------------------------
    Data List
    -------------------------------------*/
    
    if ($(".rt-featured-room").length) {
        $(window).on('load', function () {
            $("[data-list-img]:first-child").addClass("active");
        });
        $("[data-list-hover]").hover(function () {
            var t = $(this).attr("data-list-hover");
            $("[data-list-img]").removeClass("active"), $('[data-list-img="'.concat(t, '"]')).addClass("active");
        })

        // product cat menu
        var classHandler = true;
        $("#cat-button").on('click', function(){
            if(classHandler){
                $(".cat-menu-close").addClass('cat-menu-open');
            }else {
                $(".cat-menu-close").removeClass('cat-menu-open');
            }
            classHandler = !classHandler;
        });
    }
    
    /* Theia Side Bar */
    if (typeof ($.fn.theiaStickySidebar) !== "undefined") {
        $('.has-sidebar .fixed-bar-coloum').theiaStickySidebar({'additionalMarginTop': 80});
        $('.fixed-sidebar-addon .fixed-bar-coloum').theiaStickySidebar({'additionalMarginTop': 160});
    }

    if (typeof $.fn.theiaStickySidebar !== "undefined") {
    $(".sticky-coloum-wrap .sticky-coloum-item").theiaStickySidebar({
      additionalMarginTop: 130,
    });
  }

    /* Header Search */
    $('a[href="#header-search"]').on("click", function (event) {
        event.preventDefault();
        $("#header-search").addClass("open");
        $('#header-search > form > input[type="search"]').focus();
    });

    $("#header-search, #header-search button.close").on("click keyup", function (event) {
        if (
            event.target === this ||
            event.target.className === "close" ||
            event.keyCode === 27
        ) {
            $(this).removeClass("open");
        }
    });
    
    /* Mobile menu */
    $(window).on('scroll', function () {
        if ($(this).scrollTop() > 100) {
            $("body").addClass("not-top");
            $("body").removeClass("top");
        } else {
            $("body").addClass("top");
            $("body").removeClass("not-top");
        }
    });


    /* Search Box */
    $(".search-box-area").on('click', '.search-button, .search-close', function (event) {
        event.preventDefault();
        if ($('.search-text').hasClass('active')) {
            $('.search-text, .search-close').removeClass('active');
        } else {
            $('.search-text, .search-close').addClass('active');
        }
        return false;
    });

    // Advanced Search Revel
    $(".advanced-btn").on("click", function () {
        $(this).toggleClass("collapsed");
        $("#advanced-search").toggleClass("show");

    });

    /* Header Right Menu */
    var menuArea = $('.additional-menu-area');
    menuArea.on('click', '.side-menu-trigger', function (e) {
        e.preventDefault();
        var self = $(this);
        if (self.hasClass('side-menu-open')) {
            if( tripferyObj.rtl =='rtl'  ) {
                $('.sidenav').css('transform', 'translateY(0%)');
            }else {
                $('.sidenav').css('transform', 'translateY(0%)');
            }
            if (!menuArea.find('> .rt-cover').length) {
                menuArea.append("<div class='rt-cover'></div>");
            }
            self.removeClass('side-menu-open').addClass('side-menu-close');
        }
    });
	
	/*-------------------------------------
	Offcanvas Menu activation code
	-------------------------------------*/
    function closeMenuArea() {
        var trigger = $('.side-menu-trigger', menuArea);
        trigger.removeClass('side-menu-close').addClass('side-menu-open');
        if (menuArea.find('> .rt-cover').length) {
            menuArea.find('> .rt-cover').remove();
        }

        if( tripferyObj.rtl =='rtl'  ) {
            $('.sidenav').css('transform', 'translateY(100%)');
        }else {
            $('.sidenav').css('transform', 'translateY(-120%)');
        }
    }

    menuArea.on('click', '.closebtn', function (e) {
        e.preventDefault();
        closeMenuArea();
    });

    $(document).on('click', '.rt-cover', function () {
        closeMenuArea();
    });
	
    /*-------------------------------------
    MeanMenu activation code
    --------------------------------------*/
    var a = $('.offscreen-navigation .menu');
    if (a.length) {
        $(".menu-item-has-children").append("<span></span>");
        $(".page_item_has_children").append("<span></span>");

        a.children("li").addClass("menu-item-parent");

        $('.menu-item-has-children > span').on('click', function () {
            var _self = $(this),
                sub_menu = _self.parent().find('>.sub-menu');
            if (_self.hasClass('open')) {
                sub_menu.slideUp();
                _self.removeClass('open');
            } else {
                sub_menu.slideDown();
                _self.addClass('open');
            }
        });
        $('.page_item_has_children > span').on('click', function () {
            var _self = $(this),
                sub_menu = _self.parent().find('>.children');
            if (_self.hasClass('open')) {
                sub_menu.slideUp();
                _self.removeClass('open');
            } else {
                sub_menu.slideDown();
                _self.addClass('open');
            }
        });
    }
    $('.mean-bar .sidebarBtn').on('click', function (e) {
        e.preventDefault();
        $('body').toggleClass('slidemenuon');
    });

    /*Sidebar Date Placeholder*/
    if ($('#booking_date_from').length) {
        var bookingDateInput = document.getElementById('booking_date_from');
        bookingDateInput.placeholder = 'dd/mm/yyyy';
    }
    if ($('#booking_date_to').length) {
        var bookingDateInput2 = document.getElementById('booking_date_to');
        bookingDateInput2.placeholder = 'dd/mm/yyyy';
    }

    /*Header and mobile menu stick*/
    $(window).on('scroll', function () {
        if ($('body').hasClass('sticky-header')) {
            // Sticky header
            var stickyPlaceHolder = $("#sticky-placeholder"),
                menu = $("#header-menu"),
                menuH = menu.outerHeight(),
                topHeaderH = $('#tophead').outerHeight() || 0,
                middleHeaderH = $('#header-middlebar').outerHeight() || 0,
                targrtScroll = topHeaderH + middleHeaderH;
            if ($(window).scrollTop() > targrtScroll) {
                menu.addClass('rt-sticky');
                stickyPlaceHolder.height(menuH);
            } else {
                menu.removeClass('rt-sticky');
                stickyPlaceHolder.height(0);
            }

            // Sticky mobile header
            var stickyPlaceHolder = $("#mobile-sticky-placeholder"),
                menu = $(".mobile-mene-bar"),
                menuH = menu.outerHeight(),
                topHeaderH = $('#mobile-top-fix').outerHeight() || 0,
                topAdminH = $('#wpadminbar').outerHeight() || 0,
                targrtScroll = topHeaderH + topAdminH;
            if ($(window).scrollTop() > targrtScroll) {
                menu.addClass('mobile-sticky');
               stickyPlaceHolder.height(menuH);
            } else {
                menu.removeClass('mobile-sticky');
                stickyPlaceHolder.height(0);
            }
        }
    });

    /*-------------------------------------
    Thumb Slider
    -------------------------------------*/
    if ($('.largeSwiperThumb').length) {
        var largeSwiper = new Swiper(".largeSwiperThumb", {
            spaceBetween: 10,
            slidesPerView: 4,
            freeMode: true,
            watchSlidesProgress: true,
            breakpoints: {
                320: {
                    slidesPerView: 2,
                    spaceBetween: 10,
                },
                480: {
                    slidesPerView: 4,
                    spaceBetween: 10,
                },
            },
        });
        var largeSwiper = new Swiper(".largeSwiper", {
            spaceBetween: 24,
            grabCursor: true,
            thumbs: {
                swiper: largeSwiper,
            },
        });
    }

    // Popup - Used in video
    if (typeof $.fn.magnificPopup == 'function') {
        $('.rt-video-popup').magnificPopup({
            type: 'iframe',
            mainClass: 'mfp-fade',
            removalDelay: 160,
            preloader: false,
            fixedContentPos: false
        });
        $('.open-popup-link').magnificPopup({
            type: 'inline',
            midClick: true,
            mainClass: 'mfp-fade'
        });
    }
    if (typeof $.fn.magnificPopup == 'function') {
        if ($('.image-gallery').length) {
            $('.image-gallery').each(function () { // the containers for all your galleries
                $(this).magnificPopup({
                    delegate: 'a.img-grid-item', // the selector for gallery item
                    type: 'image',
                    gallery: {
                        enabled: true
                    },
                    zoom: {
                        enabled: true,
                        duration: 300, // don't foget to change the duration also in CSS
                        opener: function (element) {
                            return element.find('img');
                        }
                    }
                });
            });
        }
    }
    
    if ($(".rt-archive-slider").length) {
        var swiper = new Swiper(".rt-archive-slider", {
            slidesPerView: 4,
            autoplay: true,
            spaceBetween: 30,
            navigation: {
                nextEl: ".swiper-button-next",
                prevEl: ".swiper-button-prev",
            },
            breakpoints: {
                0: {
                    slidesPerView: 1,
                },
                576: {
                    slidesPerView: 2,
                },
                768: {
                    slidesPerView: 2,
                },
                992: {
                    slidesPerView: 3,
                },
                1200: {
                    slidesPerView: 4,
                },
                1600: {
                    slidesPerView: 4,
                },
            },
        });
    }

    /* when product quantity changes, update quantity attribute on add-to-cart button */
    $("form.cart").on("change", "input.qty", function () {
        var isgroup = $(this).parents('.woocommerce-grouped-product-list');
        if (this.value === "0"){
            if( ! isgroup.length > 0 ){
                this.value = "1";
            }
        }
        $(this.form).find("button[data-quantity]").data("quantity", this.value);
    });

    /* remove old "view cart" text, only need latest one thanks! */
    $(document.body).on("adding_to_cart", function () {
        $("a.added_to_cart").remove();
    });

    /*Quantity Product*/
    $(document).on('click', '.quantity .input-group-btn .quantity-btn', function () {
        var $input = $(this).closest('.quantity').find('.input-text');
        if ($(this).hasClass('quantity-plus')) {
            $input.trigger('stepUp').trigger('change');
        }
        if ($(this).hasClass('quantity-minus')) {
            $input.trigger('stepDown').trigger('change');
        }
    });

    $('.quantity-btn').on('click', function(){
        $("button[name='update_cart']").prop('disabled', false);
    });

    if( $('.header-shop-cart').length ){
        $( document ).on('click', '.remove-cart-item', function(){
          var product_id = $(this).attr("data-product_id");
          var loader_url = $(this).attr("data-url");
          var main_parent = $(this).parents('li.menu-item.dropdown');
          var parent_li = $(this).parents('li.cart-item');
          parent_li.find('.remove-item-overlay').css({'display':'block'});
          $.ajax({
            type: 'post',
            dataType: 'json',
            url: tripferyObj.ajaxURL,
            data: { action: "tripfery_product_remove", 
                product_id: product_id
            },success: function(data){
              main_parent.html( data["mini_cart"] );
              $( document.body ).trigger( 'wc_fragment_refresh' );
            },error: function(xhr, status, error) {
              $('.header-shop-cart').children('ul.minicart').html('<li class="cart-item"><p class="cart-update-pbm text-center">'+ tripferyObj.cart_update_pbm +'</p></li>');
            }
          });
          return false;
        }); 
    }

    //Add Wishlist
    $(document).on('click', '.wishlist', function (e) {
         e.preventDefault();
        var link = $(this);
        var post_id = link.data('post_id');
        link.addClass('ajax-loader');
        $.ajax({
            type: 'POST',
            dataType: 'json',
            url: tripferyObj.ajaxURL,
            data: {
                'action': 'tripfery_wishlist',
                'post_id': post_id,
                'mode': 'add',
                'security': tripferyObj.nonce,
            },
            success: function (data) {
                link.removeClass('ajax-loader wishlist-add');
                link.addClass('remove-wishlist');
                if (!data.logged_in) {
                    $('#form-ajax-login-popup').modal('show');
                }
                console.log(data.add_wishlist);
                if (data.add_wishlist === 'added') {
                    link.addClass('added-wishlist');
                }
            },
            error: function (data) {
                console.log('error');
            }
        });
    });

    // Mainmenu Add Class
    if ($('.site-header .main-navigation .sub-menu li a').length) {
        var menuItems = $('.site-header .main-navigation .sub-menu li a');
        menuItems.each(function () {
            var menuItemText = $(this).text();
            var newSpan = $('<span>', {
                'data-text': menuItemText,
                text: menuItemText
            });
            $(this).html(newSpan);
        });
    }

    function tripfery_search_form() {
        if ($('.rt-search-customize #search_form .input-group>div').length) {
            $('.rt-search-customize #search_form .input-group>div').wrap('<div class="parent-inner"></div>');
        }
        //search track
        if ($('.rt-search-customize #search_form_tabs').length) {
            function trackBehavior(tabContainer, target = null) {
                var selectedTrack = target ? $(target).parent().find(
                    '.search_form_tab.is-active'
                ) : tabContainer.find('.search_form_tab:first');
                var trackPosition = $(selectedTrack).position();
                tabContainer.find('.rt-scroll-btn').css({
                    left: trackPosition.left + 'px',
                    width: selectedTrack.outerWidth() + 'px',
                });
            }
       
            $('.rt-search-customize #search_form_tabs').prepend('<div class="rt-scroll-btn"></div>');
            var tabContainer = $('#search_form_tabs');
            trackBehavior(tabContainer);
            tabContainer.on('click', '.search_form_tab', function(e) {
                var target = e.currentTarget;
                trackBehavior(tabContainer,target);
            });
        }

    
        // Location Title
        if ($('.rt_location_title').length) {
            var location_title = $('.rt_location_title').text();
            $('.rt-search-customize #search_form .parent-inner:nth-child(1)').prepend('<span class="rt-title-field"> <i class="icon-tripfery-map-iocn"></i> ' + location_title + ' </span>');
        }

        // Start Date Title
        if ($('.rt_start_date').length) {
            var sd_title = $('.rt_start_date').text();
            $('.rt-search-customize #search_form .parent-inner:nth-child(2)').prepend('<span class="rt-title-field"> <i class="icon-tripfery-check-in"></i> ' + sd_title + ' </span>');
        }

        // End Date Title
        if ($('.rt_end_date').length) {
            var ed_title = $('.rt_end_date').text();
            $('.rt-search-customize #search_form .parent-inner:nth-child(4)').prepend('<span class="rt-title-field"> <i class="icon-tripfery-check-in"></i> ' + ed_title + ' </span>');
        }

        // Guests Title
        if ($('.rt_end_date').length) {
            var guests_title = $('.rt_guests').text();
            $('.rt-search-customize #search_form .parent-inner:nth-child(6)').prepend('<span class="rt-title-field"> <i class="icon-tripfery-user"></i> ' + guests_title + ' </span>');
        }
    }
    tripfery_search_form();
    // Elementor Frontend Load
    $(window).on('elementor/frontend/init', function () {
        if (elementorFrontend.isEditMode()) {
            elementorFrontend.hooks.addAction('frontend/element_ready/widget', function () {
                tripfery_search_form();
            });
        }
    });

    // Remove Wishlist
    $(document).on('click', '.remove-wishlist', function (e) {
        e.preventDefault();
        var link = $(this);
        var post_id = link.data('post_id');
        link.addClass('ajax-preload');
        $.ajax({
            type: 'POST',
            dataType: 'json',
            url: tripferyObj.ajaxURL,
            data: {
                'action': 'tripfery_wishlist',
                'post_id': post_id,
                'mode': 'remove',
                'security': tripferyObj.nonce
            },
            success: function (data) {
                link.removeClass('ajax-preload remove-wishlist');
                link.addClass('add-wishlist');
                if (!data.logged_in) {
                    $('#form-ajax-login-popup').modal('show');
                }
                console.log(data.remove_wishlist);
                if (data.remove_wishlist === 'removed') {
                    link.removeClass('added-wishlist');
                }
            },
            error: function (data) {
                console.log('error');
            }
        });
    });

});


//function Load
function tripfery_content_load_scripts() {
    var $ = jQuery;
    // Preloader
    $('#preloader').delay(1000).fadeOut('slow', function () {
        $(this).remove();
    });
    var windowWidth = $(window).width();
    imageFunction();
    function imageFunction() {
        $("[data-bg-image]").each(function () {
        let img = $(this).data("bg-image");
            $(this).css({
                backgroundImage: "url(" + img + ")",
            });
        });
    }

    /* Counter */
    var counterContainer = $('.counter');
    if (counterContainer.length) {
        counterContainer.counterUp({
            delay: counterContainer.data('rtsteps'),
            time: counterContainer.data('rtspeed')
        });
    }

    //Intersection Observer
    if (!!window.IntersectionObserver) {
    let observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add("active-animation");
                observer.unobserve(entry.target);
            }
        });
    }, {
        rootMargin: "0px 0px -100px 0px"
    });
    document.querySelectorAll('.has-animation').forEach(block => {
        observer.observe(block)
    });
    } else {
        document.querySelectorAll('.has-animation').forEach(block => {
            block.classList.remove('has-animation')
        });
    }
	
    /* Wow Js Init */
    var wow = new WOW({
        boxClass: 'wow',
        animateClass: 'animated',
        offset: 0,
        mobile: false,
        live: true,
        scrollContainer: null,
    });
    new WOW().init();

    /* Swiper slider */
    $('.rt-swiper-slider').each(function() {
        var $this = $(this);
        var settings = $this.data('xld');
        var autoplayconditon= settings['auto'];
        var $pagination = $this.find('.swiper-pagination')[0];
        var $next = $this.find('.swiper-button-next')[0];
        var $prev = $this.find('.swiper-button-prev')[0];
        var swiper = new Swiper( this, {
                autoplay: autoplayconditon,
                autoplayTimeout: settings['autoplay']['delay'],
                speed: settings['speed'],
                loop:  settings['loop'],
                pauseOnMouseEnter :true,
                slidesPerView: settings['slidesPerView'],
                spaceBetween:  settings['spaceBetween'],
                centeredSlides:  settings['centeredSlides'], 
                slidesPerGroup: settings['slidesPerGroup'],
                pagination: {
                    el: $pagination,
                    clickable: true,
                    type: 'bullets',
                },
                navigation: {
                    nextEl: $next,
                    prevEl: $prev,
                },
                breakpoints: {
                0: {
                    slidesPerView: settings['breakpoints']['0']['slidesPerView'],
                },
                576: {
                    slidesPerView: settings['breakpoints']['576']['slidesPerView'],
                },
                768: {
                    slidesPerView: settings['breakpoints']['768']['slidesPerView'],
                },
                992: {
                    slidesPerView: settings['breakpoints']['992']['slidesPerView'],
                },
                1200: {
                    slidesPerView:  settings['breakpoints']['1200']['slidesPerView'],
                },
                1600: {
                    slidesPerView: settings['breakpoints']['1600']['slidesPerView'],
                },
            },
        });
        swiper.init();
    });

    /* Horizontal Thumbnail slider */
    $('.tripfery-horizontal-slider').each(function(){
        var slider_wrap = $(this);
        var $pagination = slider_wrap.find('.swiper-pagination')[0];
        var $next = slider_wrap.find('.swiper-button-next')[0];
        var $prev = slider_wrap.find('.swiper-button-prev')[0];
        var target_thumb_slider = slider_wrap.find('.horizontal-thumb-slider');
        var thumb_slider = null;
        if(target_thumb_slider.length){
            var settings = target_thumb_slider.data('xld');
            var autoplayconditon= settings['auto'];
            thumb_slider = new Swiper(target_thumb_slider[0],
                {
                autoplay:   autoplayconditon,
                autoplayTimeout: settings['autoplay']['delay'],
                speed: settings['speed'],
                loop:  settings['loop'],
                spaceBetween:  settings['spaceBetween'],
                breakpoints: {
                    0: {
                        slidesPerView: settings['breakpoints']['0']['slidesPerView'],
                    },
                    576: {
                        slidesPerView: settings['breakpoints']['576']['slidesPerView'],
                    },
                    768: {
                        slidesPerView: settings['breakpoints']['768']['slidesPerView'],
                    },
                    992: {
                        slidesPerView: settings['breakpoints']['992']['slidesPerView'],
                    },
                    1200: {
                        slidesPerView:  settings['breakpoints']['1200']['slidesPerView'],
                    },            
                },
                pagination: {
                    el: $pagination,
                    type: "progressbar",
                },
                
            });
        }

        var target_slider = slider_wrap.find('.horizontal-slider');
        if(target_slider.length){
            var settings = target_slider.data('xld');
            new Swiper(target_slider[0], {
                autoplay:  settings && settings['auto'],
                speed: settings && settings['speed'],
                loop:  settings && settings['loop'],
                effect: settings && settings['effect'],
                thumbs: {
                  swiper: thumb_slider,
                },
                navigation: {
                    nextEl: $next,
                    prevEl: $prev,
                },
            });
        }
    });

    //ajx-tab
    $('.rt_ajax_tab a').on('click', function(e){
        e.preventDefault();
        var cat_id = $(this).data('id');
        var args = $(this).parents().data('args'); 
        var layout = $(this).parents().data('layout'); 

        if ( cat_id == '*' ) {
            cat_id = null;
        }

        var tab_content_id = $(this).closest('.rt_ajax_tab_section').next();
        $('.rt_ajax_tab a').removeClass('current');
        $(this).addClass('current');
        $.ajax({
            method: "POST",
            url: tripferyObj.ajaxURL,
            data: {
                action: 'rt_ajax_tab',
                cat_id: cat_id,
                layout: layout,
                args: args 
            },
            dataType: "json",
            beforeSend: function(){ 
                tab_content_id.prepend('<div class="preloader fa-3x"><i class="fas fa-spinner fa-spin"></i></div>'); 
            },
            success:function(res){ 
                if ( res.success ) {
                    tab_content_id.html('');
                    $(res.data).appendTo(tab_content_id).hide().fadeIn(500); 
                }
            }
        });
    });  
    
    // Replace "/
    if ($(".rt_booking_style7").length) {
        $('.rt_booking_style7 .activity-person').each(function () {
            var originalString = $(this).text();
            var replacedString = originalString.replace('/', '');
            $(this).text(replacedString);
        });
    }
}

(function ($) {
    "use strict";

    // Window Load+Resize
    $(window).on('load resize', function () {

        // Define the maximum height for mobile menu
        var wHeight = $(window).height();
        wHeight = wHeight - 50;
        $('.mean-nav > ul').css('max-height', wHeight + 'px');

        // Elementor Frontend Load
        $(window).on('elementor/frontend/init', function () {
            if (elementorFrontend.isEditMode()) {
                elementorFrontend.hooks.addAction('frontend/element_ready/widget', function () {
                    tripfery_content_load_scripts();
                });
            }
        });

    });

    // Window Load
    $(window).on('load', function () {
        tripfery_content_load_scripts();
    });

    //tripfery_single_scroll_post
    if ($(".ajax-scroll-post").length > 0) {
        var onScrollPagi = true;
        var current_post = 1;
        $(window).scroll(function () {
            if (!onScrollPagi) return;
            var bottomOffset = 1900; // the distance (in px) from the page bottom when you want to load more posts 

            if (current_post >= tripferyObj.post_scroll_limit) {
                onScrollPagi = false;
                return;
            }

            if ($(document).scrollTop() > ($(document).height() - bottomOffset) && onScrollPagi == true) {
                let cat_ids = $('input#tripfery-cat-ids').val();
                $.ajax({
                    url: tripferyObj.ajaxURL,
                    data: {
                        action: 'tripfery_single_scroll_post',
                        current_post: current_post,
                        cat_ids
                    },
                    type: 'POST',
                    dataType: 'json',
                    beforeSend: function () {
                        onScrollPagi = false;
                    },
                    success: function (resp) {
                        if (resp.success) {
                            $('.ajax-scroll-post').append(resp.data.nextContent);
                            history.pushState({}, null, resp.data.nextUrl);
                            current_post++;
                            onScrollPagi = true;
                        }
                    }
                });
            }
        });
    }
    $(window).on('scroll', scrollFunction);
    function scrollFunction() {
        var target = $('#contentHolder');
        
        if ( target.length > 0 ) {
            var contentHeight = target.outerHeight();
            var documentScrollTop = $(document).scrollTop();
            var targetScrollTop = target.offset().top;
            var scrolled = documentScrollTop - targetScrollTop;
            
            if (0 <= scrolled) {
                var scrolledPercentage = (scrolled / contentHeight) * 100;
                if (scrolledPercentage >= 0 && scrolledPercentage <= 100) {
                    scrolledPercentage = scrolledPercentage >= 90 ? 100 : scrolledPercentage;
                    $("#tripferyBar").css({
                        width: scrolledPercentage + "%"
                    });
                }
            } else {
                $("#tripferyBar").css({
                    width: "0%"
                });
            }
        }
    }

    // Ajax Blog archive layout 1 Loadmore Function
    var page = 2;
    $(document).on( 'click', '#loadMore', function( event ) {
        event.preventDefault();

        jQuery('#loadMore').addClass('loading-lazy');

        var $container = jQuery('.loadmore-items');
        $.ajax({
            type       : "GET",
            data       : {
                action: 'load_more_blog',
                numPosts : 2, 
                pageNumber: page
            },
            dataType   : "html",
            url        : tripferyObj.ajaxURL,
            success    : function(html){
            var $data = jQuery(html);
            if ($data.length) {
                $container.append( html );
                    jQuery('#loadMore').removeClass('loading-lazy');
            } else {
                jQuery("#loadMore").html("No More Blog Post"); 
                jQuery('#loadMore').removeClass('loading-lazy');
            }
            setTimeout( function() {
                revealPosts();
            }, 500);
          },
        })
        page++;
    })

    // Hover animation
    $(".hover-animation").hover(function () {
        $(".hover-animation").removeClass("hover-animation-active");
        $(this).addClass("hover-animation-active");
    });

    $(".rt-service-tab [data-list-hover]").hover(function () {
        var t = $(this).attr("data-list-hover");
        $(".rt-service-tab [data-list-hover]").removeClass("active"), $('.rt-service-tab [data-list-hover="'.concat(t, '"]')).addClass("active");
        $(".rt-service-tab [data-list-img]").removeClass("active"), $('.rt-service-tab [data-list-img="'.concat(t, '"]')).addClass("active");
    });

    $(document).ready(function () {
        /////////search form & widgets /////////
        if ($('form#search_form').length > 0) {
            $('.widget-babe-search-filter-rating').on('change', 'input:checkbox', function (ev) {
                update_ratting_values_in_search_form(this);
                babe_search_form_submit();
            });
        }

        function update_ratting_values_in_search_form(elm) {
            var rating_val = $(elm).val();
            if ($(elm).is(':checked')) {
                // append
                $('form#search_form').append('<input type="hidden" name="rating_value" value="' + rating_val + '">');
            } else {
                $('form#search_form input[type="hidden"][name="rating_value').remove();
            }
        }

        function babe_search_form_submit() {

            $('#babe_search_result_refresh').css('display', 'block');

            $('.daterangepicker .drp-calendar.left .calendar-time .input_select_field').appendTo('#search_form .input-group');
            $('.daterangepicker .drp-calendar.right .calendar-time .input_select_field').appendTo('#search_form .input-group');

            if ($('#search_form_tabs').length > 0) {
                var tab_slug = $('#search_form input[name="search_tab"]').val();
                $('#search_form div[data-inputfield="1"]:not([data-active-' + tab_slug + '])').remove();
            }
            $('#search_form input.input_select_input').removeAttr('name');
            $('#search_form .add_input_field[data-tax] .input_select_input_value').each(function (ind, elm) {
                var term_taxonomy_id = $(elm).val();
                $('#search_form input[name="terms[' + term_taxonomy_id + ']"]').remove();
                if (term_taxonomy_id != 0) {
                    // append
                    $('#search_form').append('<input type="hidden" name="terms[' + term_taxonomy_id + ']" value="' + term_taxonomy_id + '">');
                }
            });

            var args = $('#search_form').serialize();
            var action = $('#search_form').attr('action');
            var action_args = action.split('?')[1];
            var url;
            if (action_args != undefined) {
                url = action + '&' + args;
            } else {
                url = action + '?' + args;
            }

            document.location.href = url;
        }
    });

})(jQuery);




