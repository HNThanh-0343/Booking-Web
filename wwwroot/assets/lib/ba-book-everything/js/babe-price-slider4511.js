
jQuery( function( $ ) {

	// babe_price_slider is required to continue, ensure the object exists
	if ( typeof babe_price_slider === 'undefined' ) {
		return false;
	}

	// Price slider uses jquery ui
	var min_price = parseInt($( '#babe_range_price' ).data( 'min' )),
		max_price = parseInt($( '#babe_range_price' ).data( 'max' )),
		current_min_price = babe_price_slider.min_price ? parseInt( babe_price_slider.min_price ) : min_price,
		current_max_price = babe_price_slider.max_price ? parseInt( babe_price_slider.max_price ) : max_price;
        
        //////bind babe_price_slider_create babe_price_slider_slide //////

	$( document.body ).bind( 'babe_price_slider_create babe_price_slider_slide', function( event, min, max ) {
		if ( babe_price_slider.currency_pos === 'left' ) {

			$( '#babe_range_price' ).val( babe_price_slider.currency_symbol + min + ' - ' + babe_price_slider.currency_symbol + max );

		} else if ( babe_price_slider.currency_pos === 'left_space' ) {

			$( '#babe_range_price' ).val( babe_price_slider.currency_symbol + ' ' + min + ' - ' + babe_price_slider.currency_symbol + ' ' + max );

		} else if ( babe_price_slider.currency_pos === 'right' ) {

			$( '#babe_range_price' ).val( min + babe_price_slider.currency_symbol + ' - ' + max + babe_price_slider.currency_symbol );

		} else if ( babe_price_slider.currency_pos === 'right_space' ) {

			$( '#babe_range_price' ).val( min + ' ' + babe_price_slider.currency_symbol + ' - ' + max + ' ' + babe_price_slider.currency_symbol );

		}

		$( document.body ).trigger( 'babe_price_slider_updated', [ min, max ] );
	});
    
    ////////////

	$( '.babe_price_slider' ).slider({
		range: true,
		animate: true,
		min: min_price,
		max: max_price,
		values: [ current_min_price, current_max_price ],
		create: function() {

			$( document.body ).trigger( 'babe_price_slider_create', [ current_min_price, current_max_price ] );
		},
		slide: function( event, ui ) {

			$( document.body ).trigger( 'babe_price_slider_slide', [ ui.values[0], ui.values[1] ] );
		},
		change: function( event, ui ) {

			$( document.body ).trigger( 'babe_price_slider_change', [ ui.values[0], ui.values[1] ] );
		}
	});

});
