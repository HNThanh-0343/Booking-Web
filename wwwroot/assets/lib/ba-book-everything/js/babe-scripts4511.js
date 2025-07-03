(function($){
	"use strict";

////////////////////////////////////

$(document).ready(function(){
    
    /////////search form & widgets /////////

    if ($('form#search_form').length > 0){

        $('.widget-babe-search-filter-terms').on('change', 'input:checkbox', function(ev){
            update_term_values_in_search_form(this);
            babe_search_form_submit();
        });

        $('.search_advanced_field').on('change', 'input:checkbox', function(ev){
            update_term_values_in_search_form(this);
        });

        $('input[name="sr_sort_by"]').on('change', function(ev){
            $('form#search_form input[name="search_results_sort_by"]').val($(this).val());
            babe_search_form_submit();
        });

        $('.input_select_sort').on('click', function(ev){
            $('form#search_form input[name="search_results_sort_by"]').val($(this).children("input").val());
            babe_search_form_submit();
        });

        $( document.body ).bind( 'babe_price_slider_change', function( event, min, max ) {
            set_search_min_max_prices( min, max );
            babe_search_form_submit();
        });

        $('.btn-search-filter-reset').on('click', function(ev){
            $('.widget-babe-search-filter-terms input:checkbox:checked').each(function(ind, elm){
                let term_taxonomy_id = $(elm).val();
                let fieldName = $(elm).attr('name').replace('filter_', '').replace(/\[.+?]/g, '');
                $('#search_form input[name="terms['+term_taxonomy_id+']"]').remove();
                $('#search_form .add_input_field[data-tax] .input_select_input_value[name="add_ids_'+fieldName+'"]').val(0);
            });
            babe_search_form_submit();
        });
    }

    $('.babe-price-range-slider').each(function(ind, elm){
        $(elm).ionRangeSlider({
            skin: $(elm).data('skin'),
            type: $(elm).data('type'),
            prefix: $(elm).data('prefix'),
            min: $(elm).data('min'),
            max: $(elm).data('max'),
            from: $(elm).data('from'),
            to: $(elm).data('to'),
            onChange: function (data) {
                set_search_min_max_prices( data.from, data.to );
            }
        });
    });
    
    $('#search_form .submit button').on('click', function(ev){
        ev.preventDefault();
        //ev.stopPropagation();
        babe_search_form_submit();
    });
    
    $('#search_form_tabs .search_form_tab').on('click', function(ev){
        if ( !$(this).hasClass('is-active')){
            
            $('#search_form input.search_form_selected_terms').remove();

            let tab_slug = $(this).data('tab-slug');
            $('form#search_form input[name="search_tab"]').val(tab_slug);
            
            $('#search_form_tabs .search_form_tab.is-active').removeClass('is-active');
            $(this).addClass('is-active');
            
            $("#search_form div.is-active").removeClass('is-active');
            
            $(".daterangepicker .input_select_field.is-active").removeClass('is-active');
            
            $(".daterangepicker .input_select_field[data-active-"+tab_slug+"]").addClass('is-active');

            remove_search_form_daterangepicker();
            activate_search_form_daterangepicker( tab_slug );
            
            $("#search_form div[data-active-"+tab_slug+"]").each(function(ind, elm){
                let title = $(elm).data("title-"+tab_slug);
                if ($(elm).data("active-"+tab_slug) == 1){
                    $(elm).addClass('is-active');
                }
                if ( $(elm).hasClass('search-date') ){
                    $(elm).find('input').attr('placeholder',title);
                } else if ( $(elm).hasClass('add_input_field') ){
                    let selected_term = parseInt($(elm).find('input').first().val());
                    if (selected_term == 0){
                        $(elm).find('.add_ids_title_value').html(title);
                    }
                    $(elm).find('.term_item[data-id="0"]').html(title);
                } else if ($(elm).hasClass('search-keyword-block')){
                    $('#keyword').attr('placeholder',title);
                } else if($(elm).hasClass('search_guests_field')){
                    $(elm).find('.search_guests_title').html(title);
                } else if($(elm).hasClass('advanced-taxonomy-block')){
                    $(elm).find('.advanced-header').html(title);
                }
                
            });
        }
    });

    $('#search_form .search_advanced_title').on('click', function(){
        $('#search_form .search_advanced_select_wrapper').toggleClass('is-active');
        $('#search_form .search_advanced_title i.fas').toggleClass('fa-caret-down');
        $('#search_form .search_advanced_title i.fas').toggleClass('fa-caret-up');
    });
    
    $('#search_form .search_guests_title').on('click', function(){        
        
        var window_width = $(window).width();
        var dropdown = $('#search_form .search_guests_select_wrapper');
        var block_left = $(this).offset().left;
        var dropdown_width = dropdown.outerWidth();
        var css_left = dropdown_width+'px';
        
        if (block_left + dropdown_width + 20 > window_width){
            //css_left = '-'+css_left;
            dropdown.css('right', '0');
        } else {
            dropdown.css('right', 'auto');
        }
        
        $('#search_form .search_guests_select_wrapper').toggleClass('is-active');
    });

    $('#search_form .search_advanced_title').on('click', function(){

        const window_width = $(window).width();
        const dropdown = $('#search_form .search_advanced_select_wrapper');
        const block_left = $(this).offset().left;
        const dropdown_width = dropdown.outerWidth();

        if (block_left + dropdown_width + 20 > window_width){
            dropdown.css('right', '0');
        } else {
            dropdown.css('right', 'auto');
        }
    });
    
    $('#search_form .search_apply_btn').on('click', function(ev){
        ev.preventDefault();
        $('.close_by_apply_btn.is-active').removeClass('is-active');
    });
    
    $('#search_form .btn-search-guests-change').on('click', function(ev){
        let par = $(this).parent();
        let cur_val = parseInt($(par).find('.select_guests_value').first().html());
        let new_val = cur_val;
        let total_guests = 0;
        let general_guests = 0;
        let age_guests = 0;
        let guest_field_name = $(par).data('name');
        
        if ( $(this).hasClass('search_guests_plus') ){
            new_val += 1;
        }
        
        if ( $(this).hasClass('search_guests_minus') && cur_val > 0 ){
            new_val -= 1;
        }
        
        $(par).find('input.select_guests_input_value').attr('value', new_val);
        $(par).find('.select_guests_value').first().html(new_val);
        
        $('#search_form .input_select_field_guests').each(function(ind, elm){
            var cur_guest_field_name = $(elm).data('name');
            if (cur_guest_field_name != 'guests[0]'){
                age_guests += parseInt($(elm).find('.select_guests_value').first().html());
            } else {
                general_guests += parseInt($(elm).find('.select_guests_value').first().html());
            }
        });
        
        if (guest_field_name != 'guests[0]'){
            total_guests = age_guests;
            $('#search_form input.select_guests_input_value[name="guests[0]"]').attr('value', total_guests);
            $('#search_form .input_select_field_guests[data-name="guests[0]"] .select_guests_value').html(total_guests);
        } else {
            total_guests = general_guests;
            $('#search_form .input_select_field_guests:not([data-name="guests[0]"])').each(function(ind, elm){
                if (ind == 0){
                    $(elm).find('input.select_guests_input_value').attr('value', total_guests);
                    $(elm).find('.select_guests_value').html(total_guests);
                } else {
                    $(elm).find('input.select_guests_input_value').attr('value', 0);
                    $(elm).find('.select_guests_value').html('0');
                }
            });
        }
        
        $('#search_form .search_guests_title_value').html(total_guests);
        
    });
    
    /////////////////////////
    
    function babe_search_form_submit(){
        
       $('#babe_search_result_refresh').css('display', 'block');
       
       $('.daterangepicker .drp-calendar.left .calendar-time .input_select_field').appendTo('#search_form .input-group');
       $('.daterangepicker .drp-calendar.right .calendar-time .input_select_field').appendTo('#search_form .input-group');
       
       if ( $('#search_form_tabs').length > 0 ){
           let tab_slug = $('#search_form input[name="search_tab"]').val();
           $('#search_form div[data-inputfield="1"]:not([data-active-'+tab_slug+'])').remove();        
       }
       
       $('#search_form input.input_select_input').removeAttr('name');
       
       $('#search_form .add_input_field[data-tax] .input_select_input_value').each(function(ind, elm){
          let term_taxonomy_id = $(elm).val();
          $('#search_form input[name="terms['+term_taxonomy_id+']"]').remove();
          if( term_taxonomy_id != 0){
            // append
            $('#search_form').append('<input type="hidden" name="terms['+term_taxonomy_id+']" value="'+term_taxonomy_id+'">');
          }        
       });

        let args = $('#search_form').serialize();
        let action = $('#search_form').attr('action');
        let action_args = action.split('?')[1];
        let url;
        if (action_args != undefined){
             url = action + '&' + args;
        } else {
             url = action + '?' + args;
        }
          
       document.location.href = url; 
    }
    
    ///////////////////////////////
    
    $('.add_input_field .add_ids_title').on('click', function(event){
        event.preventDefault();
        search_add_input_toggle(this);
    });
    
    $('.add_input_field .add_ids_list .term_item').on('click', function(event){
        event.preventDefault();
        event.stopPropagation();
        let parent = $(this).closest('.add_ids_title');
        search_add_input_toggle(parent);

        let prev_value = $(parent).find('input.input_select_input_value').attr('value');
        $('#search_form input[name="terms['+prev_value+']"]').remove();

        $(parent).find('input.input_select_input_value').attr('value', $(this).data('id'));
        $(parent).find('.term_item').removeClass('term_item_selected');
        $(this).addClass('term_item_selected');
        $(parent).find('.add_ids_title_value').html( $(this).html().replaceAll(/<\/?[^>]+(>.*|$)/gi, "") );
    });
    
    function search_add_input_toggle(item){
        $(item).parent().find('.add_ids_list').toggleClass('active');
        $(item).parent().find('.add_ids_title i.fas').toggleClass('fa-chevron-down');
        $(item).parent().find('.add_ids_title i.fas').toggleClass('fa-chevron-up');
    }
    
    function search_add_input_close(item){
        $(item).find('.add_ids_list').removeClass('active');
        $(item).find('.add_ids_title i.fas').addClass('fa-chevron-down');
        $(item).find('.add_ids_title i.fas').removeClass('fa-chevron-up');
    }
    
    $(document).on('click', function(event){
         let par = $(event.target).closest('.add_input_field');
         if (par.length){
            $(par).siblings().each(function(ind, elm){
                search_add_input_close(this);
            });
         } else {
            $(document).find('.add_input_field .add_ids_list.active').parents().eq(1).each(function(ind, elm){
                search_add_input_close(this);
            });
         }
         let par_guests = $(event.target).closest('.search_guests_field');
         if (!par_guests.length){
            $('#search_form .search_guests_select_wrapper').removeClass('is-active');
         }
        let par_advanced = $(event.target).closest('.search_advanced_field');
        if (!par_advanced.length){
            $('#search_form .search_advanced_select_wrapper').removeClass('is-active');
        }

    });
    
    ////////////////add_datepicker/////////////////
    function add_datepicker(id) {
      $( id ).datepicker({
	    numberOfMonths: 1,
        dateFormat: babe_lst.date_format
      });
    }
    
    ///////////////search datepickers//////////////
    
    $(".search_date").on('focus', function(){
      $(this).blur();
    });
    
    $('#date_to.search_date').on('click', function(){
        $('#date_from.search_date').trigger('click.daterangepicker');
    });

    activate_search_form_daterangepicker( get_search_form_active_tab_slug() );

    ///////////////Datepicker booking form/////////////////////
    
    function is_stop_time(arr, hours){
        var count = 0;
        var stop_time = false;
        var i;
        for (i in arr) {
           if (arr.hasOwnProperty(i)) {
                count++;
                var val = arr[i];
                var h = parseInt(i);
                if (h >= hours && val == 0){
                    stop_time = true;
                }
           }
        }
        return stop_time;
    }
    
    function get_obj_prop_length(a){
        var count = 0;
        var i;
        for (i in a) {
           if (a.hasOwnProperty(i)) {
                count++;
           }
        }
        return count;
    }
    
    function get_obj_prop_max_value(a){
        var tmp_value = 0;
        var value = 0;
        var i;
        for (i in a) {
           if (a.hasOwnProperty(i)) {
               tmp_value = parseInt(a[i]);               
               if (tmp_value > value){
                  value = tmp_value;
               }
           }
        }
        return value;
    }

    var av_cal_keys = $.map(babe_lst.av_cal, function(item, key) {
            return key;
     });

    var cal_first_click = true;

    //str = JSON.stringify(obj, null, 4); // (Optional) beautiful indented output.
    //alert(str);    
          
    $(".booking_date").on('focus', function(){
      $(this).blur();
    });     
    
    $('#booking_date_from').datepicker({
        numberOfMonths: 1,
        dateFormat: babe_lst.date_format,
        minDate: babe_lst.min_av_cal_date,
        maxDate: babe_lst.max_av_cal_date,
       // defaultDate: babe_lst.date_from,
        beforeShowDay: function(date){
            let date_string = $.datepicker.formatDate('yy-mm-dd', date);
            let av_guests;
            let av_times_count;
            let check = false;
            let second_check = false;
            if ($.inArray(date_string, av_cal_keys) > -1){
                av_guests = get_obj_prop_max_value(babe_lst.av_cal[date_string]['times']);
                av_times_count = get_obj_prop_length(babe_lst.av_cal[date_string]['times']);

                if ( av_guests > 0 ){
                    check = true;
                }

                for (let rti in babe_lst.av_cal[date_string]['rates']) {
                    let rt = babe_lst.av_cal[date_string]['rates'][rti];
                    if ( parseInt(rt['start_day']) > 0 ){
                        second_check = true;
                        break;
                    }
                }

                check = check && second_check;
            }
            return [ check ]
        },
        onSelect: function(dateText, inst) {
            update_booking_time_select(true, inst);
        }
    });    
    
    var date_to_before = '';
    
    ////////////// 
    
    $('#booking_date_to').datepicker({
        numberOfMonths: 1,
        dateFormat: babe_lst.date_format,
        minDate: babe_lst.min_av_cal_date,
        maxDate: babe_lst.max_av_cal_date,
    //    defaultDate: babe_lst.date_to,
        beforeShow: function() {
                date_to_before = '';
			},
        beforeShowDay: function(date){
            var date_string = $.datepicker.formatDate('yy-mm-dd', date);
            var av_guests = 0;
            var prev_av_guests = 0;
            let check = false;
            let second_check = false;
            let from_date = $('#booking_date_from').datepicker('getDate') !== null ? $('#booking_date_from').datepicker('getDate') : '';

            if ($.inArray(date_to_before, av_cal_keys) > -1){
                prev_av_guests = get_obj_prop_max_value(babe_lst.av_cal[date_to_before]['times']);
            }
            
            if ($.inArray(date_string, av_cal_keys) > -1){
                av_guests = get_obj_prop_max_value(babe_lst.av_cal[date_string]['times']);
                if ( av_guests > 0 || (babe_lst.basic_booking_period === 'night' && prev_av_guests > 0) ){
                    check = true;
                }
            } else if(babe_lst.basic_booking_period === 'night' && prev_av_guests > 0){
                check = true;
            }

            date_to_before = date_string;

            if ( check && from_date instanceof Date){

                let booking_date_from_clicked_string = $.datepicker.formatDate('yy-mm-dd', from_date);

                if ( babe_lst.basic_booking_period === 'night' || babe_lst.basic_booking_period === 'day' ){

                    for (let rti in babe_lst.av_cal[booking_date_from_clicked_string]['rates']) {
                        let rt = babe_lst.av_cal[booking_date_from_clicked_string]['rates'][rti];
                        if ( parseInt(rt['start_day']) === 0 ){
                            continue;
                        }

                        let rate_max_booking_period = parseInt(rt['max_booking_period']);
                        let rate_min_booking_period = parseInt(rt['min_booking_period']);

                        if ( !rate_max_booking_period && !rate_min_booking_period ){
                            second_check = true;
                            break;
                        }

                        let date_check_min = new Date( from_date.getFullYear(), from_date.getMonth(), from_date.getDate() );
                        let date_check_max = new Date( from_date.getFullYear(), from_date.getMonth(), from_date.getDate() );

                        if ( rate_min_booking_period > 0 && rate_max_booking_period > 0 ){
                            date_check_min.setDate(from_date.getDate() + rate_min_booking_period);
                            date_check_max.setDate(from_date.getDate() + rate_max_booking_period);
                            if ( date >= date_check_min && date <= date_check_max ){
                                second_check = true;
                                break;
                            }
                        } else if (rate_min_booking_period > 0){
                            date_check_min.setDate(from_date.getDate() + rate_min_booking_period);
                            if ( date >= date_check_min ){
                                second_check = true;
                                break;
                            }
                        } else if (rate_max_booking_period > 0){
                            date_check_max.setDate(from_date.getDate() + rate_max_booking_period);
                            if ( date <= date_check_max ){
                                second_check = true;
                                break;
                            }
                        }
                    }

                    check = check && second_check;
                }
            }
            
            return [ check ];
        },
        onSelect: function(dateText, inst) {
            update_booking_time_select(false, inst);
        }
    });
    
    ////////////////////////////
    var init_booking_calendars = true;
    
    if (babe_lst.date_from != null){
        $('#booking_date_from').datepicker("setDate", babe_lst.date_from );
        $('.ui-datepicker-current-day').click();
        if (babe_lst.date_to != null){
          setTimeout(function(){  
           $('#booking_date_to').datepicker("setDate", babe_lst.date_to );
           $('.ui-datepicker-current-day').click();
          }, 700); 
        }
    }
    
    if (babe_lst.basic_booking_period == 'single_custom'){
        get_times_guests();
    }
    
    /////////////update_date_to_datepicker///////////////
    
    function update_date_to_datepicker(selectedYear, selectedMonth, selectedDay, date_from_updated){
        var selected_date = new Date(selectedYear, selectedMonth, selectedDay),
            start_date = new Date(selectedYear, selectedMonth, selectedDay),
            end_date = new Date(selectedYear + 1, selectedMonth, selectedDay),
            $i = 1;
            var selected_date_string = $.datepicker.formatDate('yy-mm-dd', selected_date);

        $( "#booking_date_to" ).datepicker('option', 'maxDate', end_date);
            
            var time_from = $('#booking_time_from_block .input_select_field_booking_time_from .input_select_input').length > 0 ? $('#booking_time_from_block .input_select_field_booking_time_from .input_select_input').val() : '00:00';
            time_from = parseInt(time_from);
            if ( isNaN(time_from) ){ time_from = 0; }
            var stop_time = is_stop_time(babe_lst.av_cal[selected_date_string]['times'], time_from);
            
            if (babe_lst.basic_booking_period === 'day' && stop_time){
                $( "#booking_date_to" ).datepicker('option', 'maxDate', selected_date);
            } else {
              
            start_date.setDate(start_date.getDate() + 1);
            for (var d = start_date; d <= end_date; d.setDate(d.getDate() + 1)) {
                var cur_date_string = $.datepicker.formatDate('yy-mm-dd', d);
                
                var av_guests = 0;
                var av_times_count = 0;                
                if ($.inArray(cur_date_string, av_cal_keys) > -1){
                    av_guests = get_obj_prop_max_value(babe_lst.av_cal[cur_date_string]['times']);
                    av_times_count = get_obj_prop_length(babe_lst.av_cal[cur_date_string]['times']);  
                }
                
                if (babe_lst.basic_booking_period == 'day' && av_times_count > 1){
                    if (babe_lst.av_cal[cur_date_string]['times']['00:00'] == 0){
                        d.setDate(d.getDate() - 1);
                    }
                    $( "#booking_date_to" ).datepicker('option', 'maxDate', d);
                    break;
                }                
                
                if (av_guests == 0){
                    if (babe_lst.basic_booking_period != 'night'){
                       d.setDate(d.getDate() - 1);
                    }
                    $( "#booking_date_to" ).datepicker('option', 'maxDate', d);
                    break;
                }
                
                if ( babe_lst.basic_booking_period === 'night' || babe_lst.basic_booking_period === 'day' ){

                    let rt;
                    let max_booking_period = 0;
                    for (rt in babe_lst.av_cal[selected_date_string]['rates']) {
                        let rate_max_booking_period = parseInt(babe_lst.av_cal[selected_date_string]['rates'][rt]['max_booking_period']);
                        if ( rate_max_booking_period === 0 ){
                            max_booking_period = 0;
                            break;
                        }
                        if ( rate_max_booking_period > max_booking_period){
                            max_booking_period = rate_max_booking_period;
                        }
                    }

                    if ( max_booking_period === $i ){
                        $( "#booking_date_to" ).datepicker('option', 'maxDate', d);
                        break;
                    }
                }
                
                $i++;
            } ///////////////////
            
            }

        let rt;
        let min_booking_period = 0;
        for (rt in babe_lst.av_cal[selected_date_string]['rates']) {
            let rate_min_booking_period = parseInt(babe_lst.av_cal[selected_date_string]['rates'][rt]['min_booking_period']);
            if ( rate_min_booking_period > 0){
                if ( min_booking_period === 0 ){
                    min_booking_period = rate_min_booking_period;
                } else if ( min_booking_period > rate_min_booking_period ) {
                    min_booking_period = rate_min_booking_period;
                }
            } else {
                min_booking_period = 0;
                break;
            }
        }

        if ( min_booking_period > 0 && (babe_lst.basic_booking_period === 'night' || babe_lst.basic_booking_period === 'day' ) ){
            selected_date.setDate(selected_date.getDate() + min_booking_period );
         } else if (babe_lst.basic_booking_period === 'night'){
            selected_date.setDate(selected_date.getDate() + 1);
         }
            
         $( "#booking_date_to" ).datepicker('option', 'minDate', selected_date);
         
         if (!date_from_updated && babe_lst.basic_booking_period === 'day' ){
            if ( stop_time ){
              $('#booking_date_to').datepicker("setDate", $( "#booking_date_from" ).val() );
            }  
            $('.ui-datepicker-current-day').click();
         } else {
            if (date_from_updated){
              if (init_booking_calendars){
                 init_booking_calendars = false;
              } else {
                 $('#booking_date_to').val('');
              }
            }
         }
         
    }
    
    ////////////Main AV calendar/////////////////////
    
    $('#av-cal').on('click', '.cal-cell-active:not(.cal-cell-disabled), .cal-cell-disabled-first', function(){
        $('#booking_form_total').html('');
          var Y1 = parseInt($(this).data('year')),
          m1 = parseInt($(this).data('month')) - 1,
          d1 = parseInt($(this).data('day')),
          selected_date = $(this).data('date'),
          min_booking = parseInt($(this).data('min-booking'));
          
        if (
        (!babe_lst.cal_first_click || cal_first_click)
        && $(this).hasClass('cal-cell-start-day')
        && ( babe_lst.basic_booking_period !== 'night' || (babe_lst.basic_booking_period === 'night' && !$(this).hasClass('cal-cell-disabled-first') ) )
        ){
            if (babe_lst.cal_first_click){
                //// first click
              if ($('#booking_form').length > 0 && $('#booking_date_from').val() !== selected_date){
                 $('#booking_date_from').datepicker("setDate", selected_date );
                 $('.ui-datepicker-current-day').click();
              } else {
                 av_cal_first_click_update(Y1, m1, d1);
              }
                
            } else {
                ///// or single click, select one date
              $('#booking_date_from').val(selected_date);
              if ($('#booking_form').length>0){
                $('#booking_date_from').datepicker("setDate", selected_date );
                $('.ui-datepicker-current-day').click();
                
                get_times_guests();
              }
            }
        } else {
        
          if (
        babe_lst.cal_first_click && !cal_first_click && !$(this).hasClass('cal-cell-disabled') && !$(this).hasClass('cal-cell-disabled-min') 
        && ( babe_lst.basic_booking_period !== 'day' || (babe_lst.basic_booking_period === 'day' && (min_booking == 0 || min_booking > 0 && !$(this).hasClass('cal-cell-disabled-last') && !$(this).hasClass('cal-cell-selected') ) ) )
        && ( babe_lst.basic_booking_period !== 'night' || (babe_lst.basic_booking_period === 'night' && !$(this).hasClass('cal-cell-disabled-last') ) )
        ){
            //// second click
            if ( $('#booking_form').length > 0 && $('#booking_date_to').val() !== selected_date ){
                  $('#booking_date_to').datepicker("setDate", selected_date );
                  $('.ui-datepicker-current-day').click();
                get_times_guests();
            } else {
                av_cal_second_click_update(Y1, m1, d1);
            }
          }
        }    
        
    });
    
    /// reset selected first date
    $('#av-cal').on('click', '.cal-cell-disabled', function(){
        if (babe_lst.cal_first_click){
               cal_first_click = true;
               $('#booking_date_from').val('');
               $('#booking_date_to').val('');
               $('.cal-cell-active.cal-cell-disabled, .cal-cell-disabled-first.cal-cell-disabled').removeClass('cal-cell-disabled');
               $('.cal-cell.cal-cell-selected').removeClass('cal-cell-selected');
               $('.cal-cell-active.cal-cell-disabled-min').removeClass('cal-cell-disabled-min');
        }
    });
    
    ///////////av_cal_simple_update///////////////
    
    function av_cal_simple_update(Y1, m1, d1){
        $('.cal-cell-active.cal-cell-disabled').removeClass('cal-cell-disabled');
        $('.cal-cell.cal-cell-selected').removeClass('cal-cell-selected');
        var mm = m1+1,
        dd = (d1<10)?"0"+d1:d1;
        mm = (mm<10)?"0"+mm:mm;
        var selected_date_sql = Y1+'-'+mm+'-'+dd;
        $('.cal-cell-active[data-date-sql="'+selected_date_sql+'"]').addClass('cal-cell-selected');
    }
    
    ///////////av_cal_first_click_update///////////////
    
    function av_cal_first_click_update(Y1, m1, d1){

        $('.cal-cell-active.cal-cell-disabled').removeClass('cal-cell-disabled');
        $('.cal-cell.cal-cell-selected').removeClass('cal-cell-selected');
        var mm = m1+1,
        dd = (d1<10)?"0"+d1:d1;
        mm = (mm<10)?"0"+mm:mm;
        let selected_date_sql = Y1+'-'+mm+'-'+dd;
        let current = $('.cal-cell-active[data-date-sql="'+selected_date_sql+'"]'),
          count = 0,
          stop_next_date = false;
        let selected_date = new Date( Y1, m1, d1 );

        let time_from = $('#booking_time_from_block .input_select_field_booking_time_from .input_select_input').length > 0 ? $('#booking_time_from_block .input_select_field_booking_time_from .input_select_input').val() : '00:00';
        time_from = parseInt(time_from);
        if ( isNaN(time_from) ){ time_from = 0; }
        let stop_time = is_stop_time(babe_lst.av_cal[selected_date_sql]['times'], time_from);
        
        $(current).addClass('cal-cell-selected');
        
            $('#av-cal').find('.cal-cell').not('.cal-cell-invisible').each(function(el){
                let Y = parseInt($(this).data('year')),
                m = parseInt($(this).data('month')) - 1,
                d = parseInt($(this).data('day'));
                if (Y<Y1 || (Y==Y1 && m< m1) || (Y==Y1 && m==m1 && d<d1)){
                    //// disable dates before current
                    $(this).addClass('cal-cell-disabled');
                    return;
                }
                let date = new Date(Y, m, d);

                let check = false;
                let second_check = false;

                for (let rti in babe_lst.av_cal[selected_date_sql]['rates']) {
                    let rt = babe_lst.av_cal[selected_date_sql]['rates'][rti];
                    if ( parseInt(rt['start_day']) === 0 ){
                        continue;
                    }

                    let rate_max_booking_period = parseInt(rt['max_booking_period']);
                    let rate_min_booking_period = parseInt(rt['min_booking_period']);

                    if ( !rate_max_booking_period && !rate_min_booking_period ){
                        second_check = true;
                        break;
                    }

                    let date_check_min = new Date( selected_date.getFullYear(), selected_date.getMonth(), selected_date.getDate() );
                    let date_check_max = new Date( selected_date.getFullYear(), selected_date.getMonth(), selected_date.getDate() );

                    if ( rate_min_booking_period > 0 && rate_max_booking_period > 0 ){
                        date_check_min.setDate(selected_date.getDate() + rate_min_booking_period);
                        date_check_max.setDate(selected_date.getDate() + rate_max_booking_period);
                        if ( date >= date_check_min && date <= date_check_max ){
                            second_check = true;
                            break;
                        }
                    } else if (rate_min_booking_period > 0){
                        date_check_min.setDate(selected_date.getDate() + rate_min_booking_period);
                        if ( date >= date_check_min ){
                            second_check = true;
                            break;
                        }
                    } else if (rate_max_booking_period > 0){
                        date_check_max.setDate(selected_date.getDate() + rate_max_booking_period);
                        if ( date <= date_check_max ){
                            second_check = true;
                            break;
                        }
                    }
                }

                if (
                    !check
                    && ( $(this).data('date-sql') != selected_date_sql )
                    && (
                        stop_next_date
                        || $(this).hasClass('cal-cell-disabled')
                        || $(this).hasClass('cal-cell-disabled-last')
                        || !second_check
                        || ( babe_lst.basic_booking_period === 'day'
                            && stop_time && count === 1)
                    )
                ){
                    check = true;
                    $(this).addClass('cal-cell-disabled');
                }
                if (
                    !check
                    && ( $(this).data('date-sql') != selected_date_sql )
                    && $(this).hasClass('cal-cell-stop-date')
                ){
                    stop_next_date = true;
                }
                count++;
            });
            cal_first_click = false;
    }
    
    ///////////av_cal_second_click_update///////////////
    
    function av_cal_second_click_update(Y1, m1, d1){

        let check = false;
        let from_date = $('#booking_date_from').datepicker('getDate');
        let to_date = new Date(Y1, m1, d1);

        $('#av-cal').find('.cal-cell-active, .cal-cell-disabled-first').each(function(el){
            let Y = parseInt($(this).data('year')),
                m = parseInt($(this).data('month')) - 1,
                d = parseInt($(this).data('day'));

            let date = new Date(Y, m, d);

            if ( date < from_date || date > to_date ){
                $(this).removeClass('cal-cell-selected');
                return;
            }

            $(this).addClass('cal-cell-selected');
        });
        $('.cal-cell-active.cal-cell-disabled, .cal-cell-disabled-first.cal-cell-disabled').removeClass('cal-cell-disabled');
        $('.cal-cell-active.cal-cell-disabled-min').removeClass('cal-cell-disabled-min');
        cal_first_click = true;
    }
    
    ///////////////////////////////////////
    ///////////AV calendar navigation//////
    
    $('.cal-month-bar').on('click', '.cal-month-prev', function(){
        var cur_block = $(this).parents().eq(1); //// cal-month-block
        if ($(cur_block).prev('.cal-month-block').length){
            $(cur_block).removeClass('cal-month-active');
            $(cur_block).prev('.cal-month-block').addClass('cal-month-active');
        }  
    });
    
    $('.cal-month-bar').on('click', '.cal-month-next', function(){
        var cur_block = $(this).parents().eq(1); //// cal-month-block
        if ($(cur_block).next('.cal-month-block').length){
            $(cur_block).removeClass('cal-month-active');
            $(cur_block).next('.cal-month-block').addClass('cal-month-active');
        }  
    });
    
    function av_cal_set_month(Y, m){
        var mm = m+1;
        mm = (mm<10)?"0"+mm:mm;
        $('.cal-month-block.cal-month-active').removeClass('cal-month-active');
        $('.cal-month-block[data-yearmonth="'+Y+'-'+mm+'"]').addClass('cal-month-active');
    }
    
    //////////////init booking form total price /////////
    
    booking_form_calculate();
    
    //////////////get_times_guests////////////
    
    function get_times_guests(){

        $('#booking_form_total').html('');
        let post_id = $('#booking_date_from').data('post-id'),
            date_from = $('#booking_date_from').val(),
            date_to = '';
        if ( $('#booking_date_to').length > 0 ){
            date_to = $('#booking_date_to').val();
        }

        let guests = babe_lst.guests;

        $("#booking-guests-result .select_guests").each(function(){
            let age_id = $(this).data('age-id');
            guests[age_id] = $(this).val();
        });

        $('#booking-guests-result').html('<span class="spin_f"><i class="fas fa-spinner fa-spin fa-1x"></i></span>');
        $('#booking-times').html('<span class="spin_f"><i class="fas fa-spinner fa-spin fa-1x"></i></span>');

        $.ajax({
            url : babe_lst.ajax_url,
            type : 'POST',
            data : {
                action : 'get_times_guests',
                post_id : post_id,
                date_from : date_from,
                date_to : date_to,
                guests : guests,
                // check
                nonce : babe_lst.nonce
            },
            success : function( msg ) {
                $('#booking-times').html('');
                try {
                    var response = JSON.parse( msg );
                } catch ( e ) {
                    return false;
                }
                $('#booking-times').html(response.time_lines);
                if( $('#booking-guests-result').length > 0 ){
                    $('#booking-guests-result').html(response.select_guests);
                }
                $('#booking_form .booking-services-block').html(response.services);
                booking_form_calculate();
            },
            error : function( msg ) {
                booking_form_calculate();
            }
        }).always(function(){

        });
    }
    
    ///////////on change any checkbox////////////////
    
    $('#booking_form').on('change', 'input:checkbox', function(el){
        booking_form_calculate();
    });

    $('#booking_form').on('change', 'input:radio', function(el){
        booking_form_calculate();
    });

    $('#booking_form').on('change', '.select_service_quantity', function(el){
        booking_form_calculate();
    });
    
    ///////////on change booking_time_from booking_time_to////////////////

    $('#booking_form').on('click', '.input_select_field_booking_time_from .cloud_term_item.term_item_available', function(){

        let selected_time = $(this).data('value');
        let selected_time_int = parseInt(selected_time);

        $('#booking_time_from_block').find('input').val( selected_time );
        $('#booking_time_to_block').find('input').val('');
        $('#booking_form_total').html('');

        $('#booking_time_from_block .cloud_term_item').removeClass('term_item_selected').removeClass('term_item_inbetween').removeClass('term_item_selected_from');

        $(this).addClass('term_item_selected').addClass('term_item_selected_from');

        $('#booking_time_to_block .cloud_term_item').removeClass('term_item_available').removeClass('term_item_selected').removeClass('term_item_selected_from').removeClass('term_item_inbetween');

        let from_date = $('#booking_date_from').datepicker('getDate') !== null ? $('#booking_date_from').datepicker('getDate') : '';
        let cur_date_string = $.datepicker.formatDate('yy-mm-dd', from_date);
        let found_locked_time = false;
        let found_selected_time = false;

        $('#booking_time_to_block').find('.cloud_term_item').each(function (id, el) {

            let item_time = $(el).data('value');
            let item_time_int = parseInt(item_time);
            let check = false;

            if ( !found_selected_time && item_time_int >= selected_time_int ){
                found_selected_time = true;
                if ( item_time_int === selected_time_int ){
                    $(el).addClass('term_item_selected_from');
                    return;
                }
            }

            if ( !found_selected_time || found_locked_time ){
                return;
            }

            for (let rti in babe_lst.av_cal[cur_date_string]['rates']) {
                let rt = babe_lst.av_cal[cur_date_string]['rates'][rti];
                if ( parseInt(rt['start_day']) === 0 ){
                    continue;
                }

                let rate_max_booking_period = parseInt(rt['max_booking_period']);
                let rate_min_booking_period = parseInt(rt['min_booking_period']);

                if ( !rate_max_booking_period && !rate_min_booking_period ){
                    check = true;
                    break;
                }

                if ( rate_min_booking_period > 0 && rate_max_booking_period > 0 ){

                    if (
                        (item_time_int >= selected_time_int + rate_min_booking_period)
                        && (item_time_int <= selected_time_int + rate_max_booking_period)
                    ){
                        check = true;
                        break;
                    }
                } else if (rate_min_booking_period > 0){
                    if ( item_time_int >= selected_time_int + rate_min_booking_period ){
                        check = true;
                        break;
                    }
                } else if (rate_max_booking_period > 0){
                    if ( item_time_int <= selected_time_int + rate_max_booking_period ){
                        check = true;
                        break;
                    }
                }
            }

            if ( check ){
                $(el).addClass('term_item_available');
            }

            let av_guests = parseInt(babe_lst.av_cal[cur_date_string]['times'][item_time]);
            if ( av_guests === 0 ){
                found_locked_time = true;
            }
        });
    });

    $('#booking_form').on('click', '.input_select_field_booking_time_to .cloud_term_item.term_item_available', function(){

        $('#booking_time_to_block .cloud_term_item').removeClass('term_item_selected').removeClass('term_item_inbetween');

        $(this).addClass('term_item_selected');
        $('#booking_time_to_block').find('input').val( $(this).data('value') );

        let selected_time_from = $('#booking_time_from_block .term_item_selected_from').data('value');
        let selected_time_to = $(this).data('value');
        let selected_time_from_int = parseInt(selected_time_from);
        let selected_time_to_int = parseInt(selected_time_to);

        $('#booking_time_to_block').find('.cloud_term_item').each(function (id, el) {

            let item_time = $(el).data('value');
            let item_time_int = parseInt(item_time);
            let check = false;

            if ( item_time_int > selected_time_from_int && item_time_int < selected_time_to_int ){
                $(el).addClass('term_item_inbetween');
            }
        });

        booking_form_calculate();
    });

    function init_input_cloud(){

        let from_date = $('#booking_date_from').datepicker('getDate') !== null ? $('#booking_date_from').datepicker('getDate') : '';
        let cur_date_string = $.datepicker.formatDate('yy-mm-dd', from_date);
        let found_first_available = false;

        $('#booking_time_from_block').find('.cloud_term_item').each(function (id, el) {

            let item_time = $(el).data('value');
            let item_time_int = parseInt(item_time);
            let av_guests = parseInt(babe_lst.av_cal[cur_date_string]['times'][item_time]);

            if ( !found_first_available ){
                $('#booking_time_to_block .term_item_available').each(function (idd, ell) {
                    if ( parseInt( $(ell).data('value') ) <= item_time_int ){
                        $(this).removeClass('term_item_available');
                    }
                });
            }

            if ( av_guests === 0 ){
                $(this).removeClass('term_item_available');
            } else {
                found_first_available = true;
            }
        });
    }

    $('#booking_form').on('change', '.input_select_field_booking_time_from .input_select_input_value', function(el){
        var date = $('#booking_date_from').datepicker("getDate");
        update_date_to_datepicker(date.getFullYear(), date.getMonth(), date.getDate(), false);
        update_booking_form_services();
        booking_form_calculate();
    });
    
    $('#booking_form').on('change', '.input_select_field_booking_time_to .input_select_input_value', function(el){
        update_booking_form_services();
        booking_form_calculate();
    });
    
    ///////////on change booking_time////////////////
    
    $('#booking_form').on('change', 'input:radio[name=booking_time]:checked', function(el){
        let max_select_guests = parseInt($(this).data('max-select-guests'));
        update_select_guests_block(max_select_guests);
        booking_form_calculate();
    });

    function update_select_guests_block(max_select_guests){
        let options = '';
        $("#error_group").removeClass('show_error');

        for (let i = 0; i <= max_select_guests; i++){
            var add_class = '';
            if (i == 0){
                add_class = ' term_item_selected';
            }
            options += '<li class="term_item' + add_class + '" data-id="' + i + '"  data-value="' + i + '">' + i + '</li>';
        }
        $('#booking_form').find('.select_guests_block .select_guests').val(0);
        $('#booking_form').find('.select_guests_block .input_select_list').html(options);
    }

    $('#booking_form').on('change', 'select[name=booking_time]', function(el){
        let max_select_guests = parseInt($(this).find(":selected").data('max-select-guests'));
        update_select_guests_block(max_select_guests);
        booking_form_calculate();
    });
    
    ///////////change///select_guests/////////////////
    
    $('#booking_form').on('change', 'input.select_guests', function(el){
        el.preventDefault();
        el.stopPropagation();
        var cur_selected_guests = parseInt($(this).val());
        var par = $(this).parents().eq(2); //select_guests_block
        var max_select_guests = parseInt($(par).find('.input_select_list li').last().data('id'));
        var av_guests = max_select_guests - cur_selected_guests;
        $("#error_group").removeClass('show_error');

        $(par).nextAll().each(function(ind, elm){
            var select_item = $(elm).find('.select_guests').first();
            var select_item_par = $(select_item).parent(); //input_select_wrapper
            var selected_value = parseInt($(select_item).val());
            selected_value = av_guests >= selected_value ? selected_value : av_guests;            
            var options = '';
            for (var i = 0; i <= av_guests; i++){
                var add_class = '';
                if (i == selected_value){
                    add_class = ' term_item_selected';
                }
                options += '<li class="term_item' + add_class + '" data-id="' + i + '" data-value="' + i + '">' + i + '</li>';
            }
            $(select_item).val(selected_value);
            $(select_item_par).find('.input_select_list').html(options);
            av_guests = av_guests - selected_value;
        });

        update_booking_form_services();
        booking_form_calculate();        
    });
    
    //////////////////////////////////////////////////
    
    $('#booking_form').on('change', 'input:radio[name=booking_meeting_point]:checked', function(el){
        
        var point_id = $(this).val();
        $("#error_group").removeClass('show_error');
        
        $('#block_meeting_points_default .meeting_point_default.meeting_point_default_selected').removeClass('meeting_point_default_selected');
        $('#block_meeting_points_default .meeting_point_default[data-point-id='+point_id+']').addClass('meeting_point_default_selected');
        
        $('#meeting_points_result .meeting_point.meeting_point_selected').removeClass('meeting_point_selected');
        $('#meeting_points_result .meeting_point[data-point-id='+point_id+']').addClass('meeting_point_selected');
        
    });
    
    ///////////////booking_form_calculate//////////////////////
    
    function update_booking_time_select(first_time_select, inst){
        
        var sufx = first_time_select ? 'from' : 'to';
        var start_time = first_time_select ? '00:00' : $('#booking_time_from_block .input_select_field_booking_time_from .input_select_input').val();
        var time_selected = $('.booking-time-block input[name="booking_time_'+sufx+'"]').val();
        
        if (init_booking_calendars){
            time_selected = babe_lst['time_'+ sufx];
        }
        
        let post_id = $('#booking_date_from').data('post-id'),
            date_from;

        if ( babe_lst.basic_booking_period === 'hour' ){

            date_from = $('#booking_date_from').val();

        } else {

            date_from = $('#booking_date_'+sufx).val();
        }

        if (!first_time_select && $('#booking_date_to').length > 0 && $('#booking_date_from').val() != $('#booking_date_to').val()){
           start_time = '00:00';
        }
        
        if ( date_from != '' && date_from != undefined && $('#booking_time_'+sufx+'_block').length > 0){

            $('#booking_time_'+sufx+'_block').html('<span class="spin_f"><i class="fas fa-spinner fa-spin fa-1x"></i></span>');

            $.ajax({
                url : babe_lst.ajax_url,
                type : 'POST',
                data : {
                    action : 'get_booking_times',
                    post_id : post_id,
                    date_from : date_from,
                    time_selected : time_selected,
                    start_time : start_time,
                    from_to : sufx,
                    // check
                    nonce : babe_lst.nonce
                },
                success : function( msg ) {
                    let response;
                    try {
                        response = JSON.parse( msg );
                    } catch ( e ) {
                        return false;
                    }

                    if ( babe_lst.basic_booking_period === 'hour' ){
                        $('#booking_time_from_block').html(response.booking_time_from);
                        $('#booking_time_to_block').html(response.booking_time_to);
                    } else {
                        if ( sufx === 'from' ){
                            $('#booking_time_from_block').html(response.booking_time_from);
                        } else {
                            $('#booking_time_to_block').html(response.booking_time_to);
                        }
                    }
                },
                error: function() {
                    if ( babe_lst.basic_booking_period === 'hour' ){
                        $('#booking_time_from_block').html('');
                        $('#booking_time_to_block').html('');
                    } else {
                        $('#booking_time_'+sufx+'_block').html('');
                    }
                }
            }).always(function(){
                if ( $('#booking_time_'+sufx+'_block').html() == ''){
                    cal_first_click = true;
                    $('#booking_date_from').val('');
                    $('#booking_date_to').val('');
                } else {
                    after_update_booking_time_select(first_time_select, inst);
                    update_booking_form_services();
                    booking_form_calculate();
                }

                init_input_cloud();
            });

        } else {
            after_update_booking_time_select(first_time_select, inst);
        }
    }
    
    //////////////////////////////////////////
    
    function after_update_booking_time_select(first_time_select, inst){
        
        if (first_time_select){
            if ( !babe_lst.cal_first_click || $( "#booking_date_to" ).val() !== ''){
                get_times_guests();
            }
            if (babe_lst.cal_first_click && $('#av-cal').length > 0){
                av_cal_first_click_update(inst.selectedYear, inst.selectedMonth, inst.selectedDay);
            } else {
                av_cal_simple_update(inst.selectedYear, inst.selectedMonth, inst.selectedDay);
            }
            av_cal_set_month(inst.selectedYear, inst.selectedMonth);

            update_date_to_datepicker(inst.selectedYear, inst.selectedMonth, inst.selectedDay, true);
        }
        
        if (!first_time_select && babe_lst.cal_first_click && $( "#booking_date_from" ).val() !== ''){
            if ($('#av-cal').length > 0){
               av_cal_second_click_update(inst.selectedYear, inst.selectedMonth, inst.selectedDay);
            }
            get_times_guests();
        }
        
    }
    
    ////////////booking_form_calculate//////////////////
    
    $('#booking_form').on('click', '.booking_form_calculate', function(el){
        el.preventDefault();
        el.stopPropagation();
        booking_form_calculate();        
    });
    
    ///////////////booking_form_calculate//////////////////////
    
    function booking_form_calculate(){
        let post_id = $('#booking_date_from').data('post-id'),
            date_from = $('#booking_date_from').val(),
            date_to = '',
            time_from = '',
            time_to = '';
        if ($('#booking_date_to').length > 0){
            date_to = $('#booking_date_to').val();
        } else {
            date_to = date_from;
        }
        if ($('.booking_time_line').length > 0){
            time_from = $('#booking_form input:radio[name=booking_time]:checked').val();
        } else if ($('.booking_time_line_select').length > 0){
            time_from = $('#booking_form select[name=booking_time]').val();
        }
        if ($('.input_select_field_booking_time_from').length > 0){
            time_from = $('.input_select_field_booking_time_from .input_select_input').val();
        }
        if ($('.input_select_field_booking_time_to').length > 0){
            time_to = $('.input_select_field_booking_time_to .input_select_input').val();
        }
        if (time_to == '' && $('#booking_time_to').length > 0){
            time_to = $('#booking_time_to').val();
        }

        let guests = {};
            
        $("#booking-guests-result .select_guests").each(function(){
            let age_id = $(this).data('age-id');
            guests[age_id] = $(this).val();
        });
        
        if ( date_from != '' && date_from != undefined && (!babe_lst.cal_first_click || (babe_lst.cal_first_click && date_to != '' && date_to != undefined))){
            $('#booking_form_total').html('<span class="spin_f"><i class="fas fa-spinner fa-spin fa-2x"></i></span>');
        
        $.ajax({
		url : babe_lst.ajax_url,
		type : 'POST',
		data : {
			action : 'booking_calculate',
            post_id : post_id,
            data: $('#booking_form').serialize(),
            date_from : date_from,
            date_to : date_to,
            guests : guests,
            time_from: time_from,
            time_to: time_to,
            // check
	        nonce : babe_lst.nonce
		},
		success : function( msg ) {
		    //$('#booking_form_total').html('');
            $('#booking_form_total').html(msg);
		  }
        });
        }
    }

    function update_booking_form_services(){
        let post_id = $('#booking_date_from').data('post-id'),
            date_from = $('#booking_date_from').val(),
            date_to = '',
            time_from = '',
            time_to = '';
        if ($('#booking_date_to').length > 0){
            date_to = $('#booking_date_to').val();
        }
        if ($('.booking_time_line').length > 0){
            time_from = $('#booking_form input:radio[name=booking_time]:checked').val();
        } else if ($('.booking_time_line_select').length > 0){
            time_from = $('#booking_form select[name=booking_time]').val();
        }
        if ($('.input_select_field_booking_time_from').length > 0){
            time_from = $('.input_select_field_booking_time_from .input_select_input').val();
        }
        if ($('.input_select_field_booking_time_to').length > 0){
            time_to = $('.input_select_field_booking_time_to .input_select_input').val();
        }
        if (time_to == '' && $('#booking_time_to').length > 0){
            time_to = $('#booking_time_to').val();
        }

        let guests = {};

        $("#booking-guests-result .select_guests").each(function(){
            let age_id = $(this).data('age-id');
            guests[age_id] = $(this).val();
        });

        let data = $('#booking_form').serialize();

        if ( date_from != '' && date_from != undefined && (!babe_lst.cal_first_click || (babe_lst.cal_first_click && date_to != '' && date_to != undefined))){

            $.ajax({
                url : babe_lst.ajax_url,
                type : 'POST',
                data : {
                    action : 'get_services',
                    post_id : post_id,
                    data: data,
                    date_from : date_from,
                    date_to : date_to,
                    guests : guests,
                    time_from: time_from,
                    time_to: time_to,
                    // check
                    nonce : babe_lst.nonce
                },
                success : function( msg ) {
                    if ( msg != '' ){
                        $('#booking_form .booking-services-block').html(msg);
                        $( document.body ).trigger( 'babe_booking_form_services_updated', [ data ] );
                    }
                }
            });
        }
    }
    
    ////////////booking_form_submit//////////////////
    
    $('#booking_form').on('click', '.booking_form_submit', function(el){
        el.preventDefault();
        el.stopPropagation();
        $("#error_group").removeClass('show_error');
        var post_id = $('#booking_date_from').data('post-id'),
            date_from = $('#booking_date_from').val(),
            date_to = '',
            check_meeting = 1,
            time_check = true;
        if ($('#booking_date_to').length > 0){
            date_to = $('#booking_date_to').val();
        }
        if ($('.booking_meeting_point_line').length > 0){
            var meeting_point = $('#booking_form input:radio[name=booking_meeting_point]:checked').val();
            if (meeting_point == null || meeting_point === '' || meeting_point === undefined){
                check_meeting = 0;
            }
        }

        if ( babe_lst.basic_booking_period === 'hour' ){
            time_check = $('.booking-time-block input[name="booking_time_from"]').val() !== '' && $('.booking-time-block input[name="booking_time_to"]').val() !== '';
        }

        let guests = {},
        check_guests = 1,
        guests_min = 0, guests_max = 0,
        error_message = babe_lst.messages.fill_in_all_data;
        if ($('#booking-guests-result .select_guests').length > 0){
          check_guests = 0;
          $("#booking-guests-result .select_guests").each(function(){
              guests_min = Math.max( guests_min, parseInt($(this).data('guests-min')) );
              guests_max += parseInt($(this).data('guests-max'));
              let age_id = $(this).data('age-id');
            guests[age_id] = parseInt($(this).val());
            check_guests += guests[age_id];
          });
          if ( check_guests < guests_min ){
              error_message = babe_lst.messages.minimum_guests_is+guests_min;
              check_guests = 0;
          }
            if ( check_guests > guests_max ){
                error_message = babe_lst.messages.maximum_guests_is+guests_max;
                check_guests = 0;
            }
        }
        
        if ( date_from !== '' && date_from !== undefined && check_guests > 0 && check_meeting > 0 && (!babe_lst.cal_first_click || (babe_lst.cal_first_click && date_to !== '' && date_to !== undefined)) && time_check){
           $('#booking_form').submit();
        } else {
            $("#error_group label").text(error_message);
            $("#error_group").addClass('show_error');
        }
        
    });

    $('#request_booking_form').on('submit', function(el){
        el.preventDefault();
        el.stopPropagation();

        let post_id = $('#booking_obj_id').val(),
            user_name = $('#request_booking_name').val(),
            user_email = $('#request_booking_email').val(),
            user_message = $('#request_booking_message').val();

        if ( post_id === '' || post_id === undefined || user_name === '' || user_name === undefined || user_email === '' || user_email === undefined || user_message === '' || user_message === undefined ){
            return false;
        }

        $('#request_booking_form .request_booking_form_submit').append('<span class="spin_f"><i class="fas fa-spinner fa-spin fa-2x"></i></span>');

        $.ajax({
            url : babe_lst.ajax_url,
            type : 'POST',
            data : {
                action : 'request_booking',
                post_id : post_id,
                user_name: user_name,
                user_email : user_email,
                user_message : user_message,
                date_from : $('#request_booking_date_from').val(),
                date_to : $('#request_booking_date_to').length > 0 ? $('#request_booking_date_to').val() : '',
                data: $('#request_booking_form').serialize(),
                // check
                nonce : babe_lst.nonce
            },
            success : function( msg ) {
                $('#request_booking_form .request_booking_form_submit').html(msg);
                $('#request_booking_name, #request_booking_email, #request_booking_message').val('');
                $('#request_booking_date_from, #request_booking_date_to').val('');
            }
        }).always( function(){
            $('#request_booking_form .request_booking_form_submit .spin_f').remove();
        });
    });

    $('#request_booking_date_from').datepicker({
        numberOfMonths: 1,
        dateFormat: babe_lst.date_format,
        minDate: new Date(),
        maxDate: "+2y",
        onSelect: function(dateText, inst) {
            $( "#request_booking_date_to" ).datepicker('option', 'minDate', dateText);
        }
    });

    $('#request_booking_date_to').datepicker({
        numberOfMonths: 1,
        dateFormat: babe_lst.date_format,
        minDate: new Date(),
        maxDate: "+2y"
    });
    
    ///////////select/////////////
    
    $('form, .babe_search_results_filters, .daterangepicker').on('click', '.input_select_field .input_select_title', function(event){
        event.preventDefault();
        input_select_toggle(this);
    });
    
    $("form, .babe_search_results_filters, .daterangepicker").on('focus', '.input_select_field input', function(){
      $(this).blur();
    });
    
    $('form, .babe_search_results_filters, .daterangepicker').on('click', '.input_select_field .input_select_list .term_item', function(event){
        event.preventDefault();
        event.stopPropagation();
        var element = $(this);
        var element_value = $(element).data('value');
        var element_id = $(element).data('id');
        var parent = $(element).parents().eq(1);
        var parent_title = parent.parent();
        input_select_toggle(parent_title);
        $(parent).find('.term_item').removeClass('term_item_selected');
        $(element).addClass('term_item_selected');
       // $(parent).find('input.input_select_input').attr('value', $(element).data('value'));
        $(parent).find('input.input_select_input').val($(element).data('value'));
        $(parent).find('input.input_select_input_value').attr('value', $(element).data('id'));
        $(parent).find('input.input_select_input_value').trigger('change');
    });

    function input_select_toggle(item){
        $(item).parent().find('.input_select_list').toggleClass('active');
        $(item).parent().find('.input_select_title i').toggleClass('fa-chevron-down');
        $(item).parent().find('.input_select_title i').toggleClass('fa-chevron-up');
    }
    
    function add_input_close(item){
        $(item).find('.input_select_list').removeClass('active');
        $(item).find('.input_select_title i').addClass('fa-chevron-down');
        $(item).find('.input_select_title i').removeClass('fa-chevron-up');
    }
    
    $(document).on('click', function(event){
         var par = $(event.target).closest('.input_select_field');
         if (par.length){
            $(par).siblings().each(function(el){
                add_input_close(this);
            });
         } else {
            $(document).find('.input_select_field .input_select_list.active').parents().eq(2).each(function(el){
                add_input_close(this);
            });
         }
    });
    
    /////////// checkout_form_input_field /////////////
    
    $('.checkout_form_input_field').on('focus', 'input, textarea', function(event){
        var par = $(this).parent();
        $(par).addClass('checkout_form_input_field_focus'); 
    });
    
    $('.checkout_form_input_field').on('focusout', 'input, textarea', function(event){
        var par = $(this).parent();
        
        if ($(this).val() != ''){
            $(par).addClass('checkout_form_input_field_content');
        } else {
            $(par).removeClass('checkout_form_input_field_content');
        }
        $(par).removeClass('checkout_form_input_field_focus');
    });
    
    $('.checkout_form_input_field').on('click', '.checkout_form_input_label', function(event){
        $(this).parent().find('input, textarea').trigger('focus'); 
    });

    $('#checkout_form .select2').select2();

    $('#checkout_form #billing_address_country').on('select2:select', function (e) {
        let country = $(this).val();
        let newOptions = '<option value>' + babe_lst.messages.select2select + '</option>';
        let prop_disabled = false;

        $('#checkout_form .select2_state').val('');

        if ( babe_lst.states.hasOwnProperty(country) && babe_lst.states[country].length !== 0 ){
            let select2_state_data = babe_lst.states[country];
            for(let id in select2_state_data) {
                newOptions += '<option value="'+ id +'">'+ select2_state_data[id] +'</option>';
            }
        } else {
            newOptions = '<option value>---</option>';
            prop_disabled = true;
        }

        $('#checkout_form .select2_state').select2('destroy').html(newOptions).prop("disabled", prop_disabled).select2();
    });

    $('#checkout_form .select2_state').select2();

    /////////// my account nav selector /////////////
    
    $('.my_account_page_nav_selector, .my_account_page_nav_selector_i').on('click', function(event){
        event.preventDefault();
        my_account_nav_toggle(this);
    });
    
    $('.my_account_page_nav_selector').on('focus', function(){
      $(this).blur();
    });
    
    function my_account_nav_toggle(item){
        $(item).parent().find('.my_account_page_nav_list').toggleClass('my_account_page_nav_list_active');
        $(item).parent().find('.my_account_page_nav_selector_i').toggleClass('fa-chevron-down');
        $(item).parent().find('.my_account_page_nav_selector_i').toggleClass('fa-chevron-up');
    }
    
    ///////////collapse///////////
    
    $('.collapse-title').on('click', function(){
        var par = $(this).parent();
        $(this).toggleClass('block_active');
        $(par).find('.collapse-body').first().toggleClass('block_active');
    });
    
    ///////////accordion///////////
    
    $('.accordion-title').on('click', function(){
        var par = $(this).parents('.accordion-block').eq(0);
        var par_set = $(par).parent();
        
        if ($(par).hasClass('block_active')){
            $(par).removeClass('block_active');
        } else {
            $(par_set).find('.accordion-block.block_active').removeClass('block_active');
            $(par).addClass('block_active');
        }
    });
    
    ///////////tabs///////////
    
    $('.tabs_group .tab_title').on('click', function(){
        var tabs_group = $(this).parents().eq(1);
        var tab = this;
        var method = $(this).data('method');

        if( method === 'location' ){
            babe_initMap();
        }

        $(tabs_group).find('.tab_active').removeClass('tab_active');
        $(tab).addClass('tab_active');
        $(tabs_group).find('.tab_content[data-method="'+method+'"]').first().addClass('tab_active');
    });
    
    ////////////stars//////////////////////
    
    $('.comment-form-rating-stars .star').on('mouseover', '.fa-star, svg', function(){
        var par = $(this).parent();
        var cur_star = $(par).find('.fas, .far').first();
        var cur_star_svg = $(par).find('svg').first();
        $(cur_star_svg).attr('data-prefix', 'fas');
        
        $(cur_star).addClass('fas');
        $(cur_star).removeClass('far');
        
        $(par).prevAll().each(function(){
            var star = $(this).find('.fas, .far').first();
            $(star).addClass('fas');
            $(star).removeClass('far');
            
            var star_svg = $(this).find('svg').first();
            $(star_svg).attr('data-prefix', 'fas');
        });
        
        $(par).nextAll().each(function(){
            var star = $(this).find('.fas, .far').first();
            $(star).addClass('far');
            $(star).removeClass('fas');
            
            var star_svg = $(this).find('svg').first();
            $(star_svg).attr('data-prefix', 'far');
        });        
    });
    
    $('.comment-form-rating-stars .star').on('mouseout', 'svg', function(){
        var par = $(this).parents().eq(1);
        var rating_cr = $(par).data('rating-cr');
        var rating_val = parseInt($('#rating_'+rating_cr).val());
        
        $(par).find('.star').each(function(){
            var star = $(this).find('.fas, .far').first();
            var star_svg = $(this).find('svg').first();
            
            var star_val = parseInt($(this).data('rating-val'));
            
            if (star_val <= rating_val){
                $(star).addClass('fas');
                $(star).removeClass('far');
                $(star_svg).attr('data-prefix', 'fas');
            } else {
                $(star).addClass('far');
                $(star).removeClass('fas');
                $(star_svg).attr('data-prefix', 'far');
            }
            
        }); 
      });
    
      $('.comment-form-rating-stars .star').on('mousedown', 'svg', function(){    
        var rating_cr = $(this).parents().eq(1).data('rating-cr');
        var star_val = parseInt($(this).parent().data('rating-val'));
        $('#rating_'+rating_cr).val(star_val);     
      });
    
      $('.comment-form-rating-stars').on('mouseout', function(){
        var rating_cr = $(this).data('rating-cr');
        var rating_val = parseInt($('#rating_'+rating_cr).val());
        
        $(this).find('.star').each(function(){
            var star = $(this).find('.fas, .far').first();
            var star_svg = $(this).find('svg').first();
            
            var star_val = parseInt($(this).data('rating-val'));
            
            if (star_val <= rating_val){
                $(star).addClass('fas');
                $(star).removeClass('far');
                $(star_svg).attr('data-prefix', 'fas');
            } else {
                $(star).addClass('far');
                $(star).removeClass('fas');
                $(star_svg).attr('data-prefix', 'far');
            }
            
        }); 
      });
    
      $('.comment-form-rating-stars .star').on('click', '.fa-star', function(){    
        var rating_cr = $(this).parents().eq(1).data('rating-cr');
        var star_val = parseInt($(this).parent().data('rating-val'));
        $('#rating_'+rating_cr).val(star_val);      
      });
    
    ///////////////Coupon apply/////////
    
    $('#coupon_form_submit').click(function(){
        let coupon_num = $('#coupon_input_field').val();
         
         if (coupon_num){
            
            $('#coupon_form_submit_loader').html('<span class="spin_f"><i class="fas fa-spinner fa-spin fa-2x"></i></span>');      
        $.ajax({
		url : babe_lst.ajax_url,
		type : 'POST',
		data : {
			action : 'apply_coupon_to_order',
            order_id : getUrlParameter('order_id'),
            order_num : getUrlParameter('order_num'),
            order_hash : getUrlParameter('order_hash'),
            coupon_num: coupon_num,
            // check
	        nonce : babe_lst.nonce
		},
		success : function( msg ) {
            //////reload page
            window.location.reload(true);
		  },
        error : function(){
            $('#coupon_form_submit_loader').html('');
        }  
        });
         }
    });

    $('#remove_coupon_form_submit').click(function(){
        let coupon_num = $(this).data('coupon-num');
        if (coupon_num){
            $('#coupon_form_submit_loader').html('<span class="spin_f"><i class="fas fa-spinner fa-spin fa-2x"></i></span>');
            $.ajax({
                url : babe_lst.ajax_url,
                type : 'POST',
                data : {
                    action : 'remove_coupon_from_order',
                    order_id : getUrlParameter('order_id'),
                    order_num : getUrlParameter('order_num'),
                    order_hash : getUrlParameter('order_hash'),
                    coupon_num: coupon_num,
                    // check
                    nonce : babe_lst.nonce
                },
                success : function( msg ) {
                    //////reload page
                    window.location.reload(true);
                },
                error : function(){
                    $('#coupon_form_submit_loader').html('');
                }
            });
        }
    });
    
    ////////////unitegallery///////////////
    
    if ($('#unitegallery').length > 0){
        $("#unitegallery").unitegallery( babe_lst.unitegallery_args );
    }    
    
    ///////////payment method to input///////////
    
    $('.payment_group .payment_method_title').on('click', function(){
        var tabs_group = $(this).parents().eq(1);
        var method = $(this).data('method');
        var order_id = getUrlParameter('order_id');
        $(tabs_group).find('input[name="payment[payment_method]"]').first().val(method);

        $('#babe_search_result_refresh').css('display', 'block');

        $.ajax({
            url : babe_lst.ajax_url,
            type : 'POST',
            data : {
                action : 'checkout_payment_method_changed',
                order_id : order_id,
                order_num : getUrlParameter('order_num'),
                order_hash : getUrlParameter('order_hash'),
                payment_method: method,
                // check
                nonce : babe_lst.nonce
            },
            success : function( msg ) {
                try {
                    var response = JSON.parse( msg );
                } catch ( e ) {
                    return false;
                }
                if ( response.order_items != '' && response.amount_group != '' ){

                    $('#checkout_form_block').find('.coupon-form-block-applied').remove();
                    $('#checkout_form_block').find('.table_order_items_details').first().replaceWith(response.order_items);
                    $('#checkout_form_block').find('.amount_group').first().replaceWith(response.amount_group);
                    if ( response.amount_changed != 0 ){
                        $('html, body').animate({
                            scrollTop: $('#checkout_form_block').offset().top - 100
                        }, 500);
                    }
                    $( document.body ).trigger( 'babe_checkout_payment_method_changed', [ method, order_id ] );
                } else {
                    //////reload page
                    window.location.reload(true);
                }
            },
            error : function(){
            }
        }).always( function(){
            $('#babe_search_result_refresh').css('display', 'none');
        });
    });

    $( document.body ).bind( 'babe_checkout_payment_gateway_submit', function( event, method ) {
        $('#babe_search_result_refresh').css('display', 'block');
    });

    $('#checkout_form').on('submit', function() {
        var payment_method = $('#checkout_form input[name="payment[payment_method]"]').val();
        let form_checked = true;
        $('#checkout_form input[required]').each(function(ind, elm){
            if ( $(elm).val() == '' ){
                form_checked = false;
            }
        });
        if ( $('#order_terms_check').length > 0 && !$('#order_terms_check').is(':checked') ){
            form_checked = false;
        }
        if ( form_checked && (payment_method === 'cash' || payment_method === 'paypal') ){
            $( document.body ).trigger( 'babe_checkout_payment_gateway_submit', [ payment_method ] );
        }
    });
    
    ////////////validate////////////////
    
    $( "#checkout_form" ).validate({
        rules: {
            email_check: {
                required: true,
                email: true,
                equalTo: "#email",
            }
        }
    });
    
    //////////////////Google API///

    if (  babe_lst.google_map_active == 1 ){

        babe_initMap();

        $( document.body ).bind( 'babe_google_map_meeting_points_to_init', function( event ){
            init_meeting_map('#google_map_meeting_points', '#block_meeting_points .address-autocomplete', '#block_meeting_points .find_points');
        });

        //////////////////////////////

        $( document.body ).bind( 'babe_google_map_with_direction_to_init', function( event ){
            init_address_map_with_direction('#google_map_address_with_direction', '#block_address_map_with_direction .address-autocomplete', '#travel_mode_panel');
        });

        $('#booking_form').on('click', '.booking_meeting_point_line a[open-mode="modal"]', function( event ){

            event.stopPropagation();
            event.preventDefault();

            var gmap_div = $('#google_map_address_with_direction');

            $(gmap_div).data('obj-id', $(this).data('obj-id'));
            $(gmap_div).data('lat', $(this).data('lat'));
            $(gmap_div).data('lng', $(this).data('lng'));
            $(gmap_div).data('address', $(this).data('address'));

            $('#block_address_map_with_direction').append(babe_lst.travel_mode_html);

            babe_overlay_open();

            init_address_map_with_direction('#google_map_address_with_direction', '#block_address_map_with_direction .address-autocomplete', '#travel_mode_panel_modal');
        });

        $('#route_to_button_ok').on('click', function( event ){
            babe_overlay_close();
        });
    }

    ///////////////////////
    ///////My account///////////////////////
    
    $('.my_account_my_bookings_table_td').on('click', '.my_bookings_table_a_expand', function(el){
       el.stopPropagation();
       el.preventDefault();
       var order_id = $(this).data('order-id');
       $('.my_bookings_table_td_expand[data-order-id="'+order_id+'"]').toggleClass('show_td_expand');
       
    });
    
    $( "#registration_form" ).validate({
        rules: {
            new_email: {
                required: true,
                email: true,
                equalTo: "#new_email_confirm",
            }
        }
    });
    
    $( "#edit_user_profile .edit_user_profile_submit" ).on('click', function(event){
        
        event.preventDefault();
        event.stopPropagation();
        
        $('#edit_user_profile .form-spinner').css('display', 'block');      
        $.ajax({
		url : babe_lst.ajax_url,
		type : 'POST',
		data : {
			action : 'check_free_username',
            username : $( "#edit_user_profile #username" ).val(),
            // check
	        nonce : babe_lst.nonce
		},
		success : function( msg ) {
            $('#edit_user_profile .form-spinner').css('display', 'none');
            if (msg != ''){
                $( "#edit_user_profile" ).submit();
            } else {
                $('.new-username-check-msg').css('display', 'block');

                var $root = $('html, body');
                $root.animate({
                    scrollTop: $('#new-username-check').offset().top - 150
                    }, 500);
            }
		  },
        error : function(){
            $('#edit_user_profile .form-spinner').css('display', 'none');
            $( "#edit_user_profile" ).submit();
        }  
        });
        
    });
    
    $( "#edit_user_profile #username, #registration_form #new_username" ).on('focus', function(event){
        $('.new-username-check-msg').css('display', 'none');
    });
    
    $( "#edit_user_profile #email, #registration_form #new_email" ).on('focus', function(event){
        $('.new-email-check-msg').css('display', 'none');
    });
    
    $( "#registration_form #new-submit" ).on('click', function(event){
        
        event.preventDefault();
        event.stopPropagation();
        
        $('#registration_form .form-spinner').css('display', 'block');      
        $.ajax({
		url : babe_lst.ajax_url,
		type : 'POST',
		data : {
			action : 'check_free_username_email',
            username : $( "#registration_form #new_username" ).val(),
            email : $( "#registration_form #new_email" ).val(),
            // check
	        nonce : babe_lst.nonce
		},
		success : function( msg ) {
            $('#registration_form .form-spinner').css('display', 'none');
            try {
			var response = JSON.parse( msg );
		    } catch ( e ) {
			  return false;
		    }
            if (response.username == ''){
                $('.new-username-check-msg').css('display', 'block');             
            }
            if (response.email == ''){
                $('.new-email-check-msg').css('display', 'block');             
            }
            if (response.username != '' && response.email != ''){
                $( "#registration_form" ).submit();
            }
		  },
        error : function(){
            $('#registration_form .form-spinner').css('display', 'none');
            $( "#registration_form" ).submit();
        }  
        });
        
    });
    
    if ($('#user_registered_modal').length > 0){
        $('#user_registered_modal').modal('show');
    }
    
    ///////////////////////////////

});

    ////////////update_current_meeting_point/////////////

    function update_current_meeting_point(point_id){
        $('#booking_form input:radio[name=booking_meeting_point]:checked').prop('checked', false);
        $('#booking_form input#booking_meeting_point_'+point_id).prop('checked', true);

        $('#block_meeting_points_default .meeting_point_default.meeting_point_default_selected').removeClass('meeting_point_default_selected');
        $('#block_meeting_points_default .meeting_point_default[data-point-id='+point_id+']').addClass('meeting_point_default_selected');

        $('#meeting_points_result .meeting_point.meeting_point_selected').removeClass('meeting_point_selected');
        $('#meeting_points_result .meeting_point[data-point-id='+point_id+']').addClass('meeting_point_selected');
    }

///////////////

    function update_term_values_in_search_form(elm){
        let term_taxonomy_id = $(elm).val();
        if($(elm).is(':checked')){
            // append
            $('form#search_form').append('<input type="hidden" name="terms['+term_taxonomy_id+']" value="'+term_taxonomy_id+'">');
        } else {
            // unchecked
            let fieldName = $(elm).attr('name').replace('filter_', '').replace(/\[.+?]/g, '');
            $('#search_form input[name="terms['+term_taxonomy_id+']"]').remove();
            $('#search_form .add_input_field[data-tax] .input_select_input_value[name="add_ids_'+fieldName+'"]').val(0);
        }
    }

///////////////

    function set_search_min_max_prices( min, max ) {
        if ($('form#search_form').length > 0) {
            if ($('form#search_form input[name="min_price"]').length > 0) {
                $('form#search_form input[name="min_price"]').val(min);
            } else {
                $('form#search_form').append('<input type="hidden" name="min_price" value="' + min + '">');
            }
            if ($('form#search_form input[name="max_price"]').length > 0) {
                $('form#search_form input[name="max_price"]').val(max);
            } else {
                $('form#search_form').append('<input type="hidden" name="max_price" value="' + max + '">');
            }
        }
    }


///////////////

function babe_initMap(){

    if ($('#block_address_map').length > 0 && babe_lst.google_map_active == 1){
       init_address_map('#google_map_address');
    }
    
    if ($('#block_meeting_points').length > 0 && babe_lst.google_map_active == 1){
       //$( document.body ).trigger( 'babe_google_map_meeting_points_to_init' );
       init_meeting_map('#google_map_meeting_points', '#block_meeting_points .address-autocomplete', '#block_meeting_points .find_points');
    }
    
    if ($('#block_address_map_with_direction').length > 0 && babe_lst.google_map_active == 1){
       //$( document.body ).trigger('babe_google_map_with_direction_to_init');
       init_address_map_with_direction('#google_map_address_with_direction', '#block_address_map_with_direction .address-autocomplete', '#travel_mode_panel');
    }
    
}

////////////////

function getUrlParameter(sParam) {
    var sPageURL = decodeURIComponent(window.location.search.substring(1)),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : sParameterName[1];
        }
    }
    
    return '';
}

//////////////make route/////////////////////
    
    function make_route(directionsRenderer, directionsService, origin, destination){
        var selectedMode = $('#travel_mode').val();
        directionsService.route({
          origin: origin,
          destination: destination,
          // Note that Javascript allows us to access the constant
          // using square brackets and a string value as its
          // "property."
          travelMode: google.maps.TravelMode[selectedMode]
        }, function(response, status) {
          if (status == 'OK') {
              directionsRenderer.setDirections(response);
          } else {
            // window.alert('Directions request failed due to ' + status);
          }
        });
    }

/////////////init_address_map////////////////
    
    async function init_address_map(map_div){
        
        var dom_obj = $(map_div)[0];
        var post_id = $(map_div).data('obj-id');
        var var_lat = parseFloat($(map_div).data('lat'));
        var var_lng = parseFloat($(map_div).data('lng'));
        var address = $(map_div).data('address');

        const { Map } = await google.maps.importLibrary("maps");
        const { AdvancedMarkerElement, PinElement } = await google.maps.importLibrary("marker");
        
        var map = new Map(dom_obj, {
          center: {lat: var_lat, lng: var_lng},
        //  mapTypeControl: false,
        //  panControl: false,
        //  streetViewControl: false,
          zoom: parseInt(babe_lst.start_zoom),
            mapId: 'DEMO_MAP_ID',
        });
        
        var infowindow = new google.maps.InfoWindow();

        const glyphImg = document.createElement("img");
        glyphImg.src = babe_lst.marker_icon;
        const pin = new PinElement({
            glyph: glyphImg,
        });

        var marker = new AdvancedMarkerElement({
            map: map,
            position: {lat: var_lat, lng: var_lng},
            content: pin.element,
        });

          infowindow.setContent('<div>' + address + '</div>');
          infowindow.open(map, marker);
    }
    
//////////init_address_map_with_direction////////////////
    
    async function init_address_map_with_direction(map_div, autocomplete_selector, travel_mode_panel_selector){
        
        var dom_obj = $(map_div)[0];
        var post_id = $(map_div).data('obj-id');
        var var_lat = parseFloat($(map_div).data('lat'));
        var var_lng = parseFloat($(map_div).data('lng'));
        var address = $(map_div).data('address');
        
        if (post_id != ''){

            const { Map } = await google.maps.importLibrary("maps");
            const {DirectionsService, DirectionsRenderer} = await google.maps.importLibrary("routes");
            const { AdvancedMarkerElement, PinElement } = await google.maps.importLibrary("marker");

            var directionsRenderer = new DirectionsRenderer();
            var directionsService = new DirectionsService();
        
        var map = new Map(dom_obj, {
          center: {lat: var_lat, lng: var_lng},
          mapTypeControl: false,
          panControl: false,
          streetViewControl: false,
          zoom: parseInt(babe_lst.start_zoom),
            mapId: 'DEMO_MAP_ID_2',
        });

            directionsRenderer.setMap(map);
        
        var input = $(autocomplete_selector)[0];

        map.controls[google.maps.ControlPosition.TOP_LEFT].push($(travel_mode_panel_selector)[0]);
        map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);

        var autocomplete = new google.maps.places.Autocomplete(input, {
              types: []
            });
        autocomplete.bindTo('bounds', map);
        
        var infowindow_from = new google.maps.InfoWindow();
        var infowindow_to = new google.maps.InfoWindow();

            const glyphImg = document.createElement("img");
            glyphImg.src = babe_lst.marker_icon;
            const pin = new PinElement({
                glyph: glyphImg,
            });

            var marker_args = {
                map: map,
                content: pin.element,
            };

            var marker_from = new AdvancedMarkerElement(marker_args);
            var marker_to = new AdvancedMarkerElement(marker_args);
        
        marker_from.position = {lat: var_lat, lng: var_lng};
        
        infowindow_from.setContent('<div>' + address + '</div>');
        infowindow_from.open(map, marker_from);
        
        google.maps.event.addListener(marker_from, 'click', function(){
           infowindow_from.open(map, marker_from);
        });
        
        google.maps.event.addListener(marker_to, 'click', function(){
           infowindow_to.open(map, marker_to);
        });
        
        var selected_lat, selected_lng;

        autocomplete.addListener('place_changed', function() {
          infowindow_to.close();
            marker_to.visible = false;

          var place = autocomplete.getPlace();
          
          // If the place has a geometry, then present it on a map.
          if (place.geometry.viewport) {
            map.fitBounds(place.geometry.viewport);
          } else {
            map.setCenter(place.geometry.location);
            //map.setZoom(17);  // Why 17? Because it looks good.
          }
          
          selected_lat = place.geometry.location.lat();
          selected_lng = place.geometry.location.lng();
          
          marker_to.position = place.geometry.location;
            marker_to.visible = true;

          var address = '';
          var selected_address = '';
          if (place.address_components) {
            address = [
              (place.address_components[0] && place.address_components[0].short_name || ''),
              (place.address_components[1] && place.address_components[1].short_name || ''),
              (place.address_components[2] && place.address_components[2].short_name || ''),
              (place.address_components[4] && place.address_components[4].short_name || ''),
              (place.address_components[6] && place.address_components[6].long_name || '')
            ].join(', ');
            selected_address = $(autocomplete_selector).val();
          }

          infowindow_to.setContent('<div><strong>' + place.name + '</strong><br>' + address + '</div>');
          infowindow_to.open(map, marker_to);
          
          var destination = {lat: var_lat, lng: var_lng};
          var origin = {lat: selected_lat, lng: selected_lng};
          make_route(directionsRenderer, directionsService, origin, destination);
          
        });
        
        }      
        
    }
    
/////////////init_meeting_map////////////////
    
    async function init_meeting_map(map_div, autocomplete_selector, button_go){
        
        var dom_obj = $(map_div)[0];
        var post_id = $(map_div).data('obj-id');
        var var_lat = parseFloat($(map_div).data('lat'));
        var var_lng = parseFloat($(map_div).data('lng'));
        
        if (var_lat == '' || var_lat == undefined || var_lat == null || isNaN(var_lat)){
            var_lat = parseFloat(babe_lst.start_lat);
        }
        
        if (var_lng == '' || var_lng == undefined || var_lng == null || isNaN(var_lng)){
            var_lng = parseFloat(babe_lst.start_lng);
        }

        const { Map } = await google.maps.importLibrary("maps");
        const { DirectionsService, DirectionsRenderer } = await google.maps.importLibrary("routes");
        const { AdvancedMarkerElement, PinElement } = await google.maps.importLibrary("marker");

        var directionsRenderer = new DirectionsRenderer();
        var directionsService = new DirectionsService();
        
        var map = new Map(dom_obj, {
          center: {lat: var_lat, lng: var_lng},
          mapTypeControl: false,
          panControl: false,
          streetViewControl: false,
          zoom: parseInt(babe_lst.start_zoom),
          mapId: 'DEMO_MAP_ID',
        });

        directionsRenderer.setMap(map);
        
        var input = $(autocomplete_selector)[0];

        map.controls[google.maps.ControlPosition.TOP_LEFT].push($('#travel_mode_panel')[0]);
        //map.controls[google.maps.ControlPosition.TOP_LEFT].push($(button_go)[0]);

        var autocomplete = new google.maps.places.Autocomplete(input, {
              types: []
            });
        autocomplete.bindTo('bounds', map);

        var infowindow = new google.maps.InfoWindow();

        const glyphImg = document.createElement("img");
        glyphImg.src = babe_lst.marker_icon;
        const pin = new PinElement({
            glyph: glyphImg,
        });

        var marker_args = {
          map: map,
          content: pin.element,
        };
        
        //var marker_from = new google.maps.Marker(marker_args);
        var marker_to = new AdvancedMarkerElement(marker_args);
        //marker_from.setPosition({lat: var_lat, lng: var_lng});
        
        var selected_lat, selected_lng;

        autocomplete.addListener('place_changed', function() {
          infowindow.close();
          marker_to.visible = false;
          var place = autocomplete.getPlace();
          
          // If the place has a geometry, then present it on a map.
          if (place.geometry.viewport) {
            map.fitBounds(place.geometry.viewport);
          } else {
            map.setCenter(place.geometry.location);
            map.setZoom(17);  // Why 17? Because it looks good.
          }
          
          selected_lat = place.geometry.location.lat();
          selected_lng = place.geometry.location.lng();
          
          marker_to.position = place.geometry.location;
          marker_to.visible = true;

          var address = '';
          var selected_address = '';
          if (place.address_components) {
            address = [
              (place.address_components[0] && place.address_components[0].short_name || ''),
              (place.address_components[1] && place.address_components[1].short_name || ''),
              (place.address_components[2] && place.address_components[2].short_name || ''),
              (place.address_components[4] && place.address_components[4].short_name || ''),
              (place.address_components[6] && place.address_components[6].long_name || '')
            ].join(', ');
            selected_address = $(autocomplete_selector).val();
          }

          infowindow.setContent('<div><strong>' + place.name + '</strong><br>' + address + '</div>');
          infowindow.open(map, marker_to);
          
          $(button_go).on('click', function(el){
             el.stopPropagation();
             el.preventDefault();
             //// get_meeting_points
             $('#meeting_points_result').html('<span class="spin_f"><i class="fas fa-spinner fa-spin fa-2x"></i></span>');      
        $.ajax({
		url : babe_lst.ajax_url,
		type : 'POST',
		data : {
			action : 'get_meeting_points',
            post_id : post_id,
            lat: selected_lat,
            lng: selected_lng,
            // check
	        nonce : babe_lst.nonce
		},
		success : function( msg ) {
		  $('#meeting_points_result').html(msg);
            ///////////////    
		  }
        });
          });
          
          $('#meeting_points_result').on('click', '.add_destination', function(el){
             el.stopPropagation();
             el.preventDefault();
             var llat = $(this).data('lat'),
                 llng = $(this).data('lng'),
                 address = $(this).data('address'),
                 point_id = $(this).data('point-id');
             var destination = {lat: parseFloat(llat), lng: parseFloat(llng)};
             var origin = {lat: selected_lat, lng: selected_lng};
             make_route(directionsRenderer, directionsService, origin, destination);
             update_current_meeting_point(point_id);
          });
          
        });      
        
    }

    /////////////////////////////////////////

    function init_search_form_daterangepicker( is_single ){

        if ( !$('#date_from.search_date').length ){
            return;
        }

        $('#date_from.search_date').daterangepicker({
            minDate: moment().format(babe_lst.drp_date_format),
            singleDatePicker: is_single,
            autoApply: true,
            autoUpdateInput: false,
            dateFormat: babe_lst.drp_date_format,
            customClass: 'search-popup-date',
            widthSingle: 500,
            timePickerIncrement: 60,
            timePickerSeconds: false,
            timePicker: false,
            locale: {
                "format": babe_lst.drp_date_format,
                "separator": " - ",
                "customRangeLabel": babe_lst.daterangepickerLocale.customRangeLabel,
                "weekLabel": babe_lst.daterangepickerLocale.weekLabel,
                "applyLabel": babe_lst.daterangepickerLocale.applyLabel,
                "cancelLabel": babe_lst.daterangepickerLocale.cancelLabel,
                "fromLabel": babe_lst.daterangepickerLocale.fromLabel,
                "toLabel": babe_lst.daterangepickerLocale.toLabel,
                "daysOfWeek": Object.keys(babe_lst.daterangepickerLocale.daysOfWeek).map(function (key) { return babe_lst.daterangepickerLocale.daysOfWeek[key]; }),
                "monthNames": Object.keys(babe_lst.daterangepickerLocale.monthNames).map(function (key) { return babe_lst.daterangepickerLocale.monthNames[key]; }),
                "firstDay": babe_lst.daterangepickerLocale.firstDay
            }
        });

        if ( babe_lst.date_from != null){
            $('#date_from.search_date').data('daterangepicker').setStartDate(babe_lst.date_from);
        }
        if ( babe_lst.date_to != null){
            $('#date_from.search_date').data('daterangepicker').setEndDate(babe_lst.date_to);
        }

        $('#date_from.search_date').on('apply.daterangepicker', function(ev, picker) {
            // var time_from = $('.babe-search-form').find("input[name='time_from']").val();
            // var time_to = $('.babe-search-form').find("input[name='time_to']").val();
            $(this).val(picker.startDate.format(babe_lst.drp_date_format));
            $('#date_to.search_date').val(picker.endDate.format(babe_lst.drp_date_format));
        });
    }

    function remove_search_form_daterangepicker(){
        $('#date_from.search_date').data('daterangepicker');
    }

    function activate_search_form_daterangepicker( tab_slug ){
        var date_to_active = $("#search_form .search_date_wrapper_date_to[data-active-"+tab_slug+"]").data("active-"+tab_slug) != undefined ? true : false;
        var is_single = !date_to_active;
        init_search_form_daterangepicker( is_single );
    }

    function get_search_form_active_tab_slug(){
        var tab_slug = $('#search_form_tabs .search_form_tab.is-active').data('tab-slug');
        if (tab_slug == undefined){
            tab_slug = '0';
        }
        return tab_slug;
    }
    
////////////////////////

})(jQuery);
