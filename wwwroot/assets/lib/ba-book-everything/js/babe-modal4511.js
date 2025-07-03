
jQuery(function($){
           
      $('#modal_close, #babe_overlay, .babe_overlay_inner #close, .babe_overlay_inner #cancel').click(function(event) {
              babe_overlay_close();
      });
      
      //////////
});

function babe_overlay_close(){
           jQuery('#babe_overlay_container').animate({opacity: 0}, 200,	function(){
					jQuery('#babe_overlay_container').css('display', 'none');
					jQuery('#babe_overlay').fadeOut(400);
				}
		   );
}

function babe_overlay_open(){
    var top = jQuery(document).scrollTop();
   // jQuery('#babe_overlay_container').css('top', top + 'px');
    jQuery('#babe_overlay_container').css('display', 'block');
    jQuery('#babe_overlay').fadeIn(400, function(){
				jQuery('#babe_overlay_container')
					.animate({opacity: 1}, 200);
	});
} 
