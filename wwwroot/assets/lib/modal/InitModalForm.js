var initFormModal;
$(function () {

    $.ajaxSetup({ cache: false });
    $(document).off("click", "a[data-toggle='modal']")
        .on("click", "a[data-toggle='modal']", function (e) {
            e.preventDefault(); // Prevent the default link behavior        
            var sizeModal = $(this).data("sizemodal");
            // Nếu khác true thì xóa class modal-lg
            $("#changeSizeModal").attr("class", "modal-dialog");
            if (sizeModal) {
                $("#changeSizeModal").addClass(sizeModal);
            } else {
                $("#changeSizeModal").addClass("modal-lg");
            }

            //if (sizeModal !== true) {
            //    $("#changeSizeModal").removeClass("modal-lg");
            //} else {
            //    $("#changeSizeModal").addClass("modal-lg");
            //}
            $('#myModalContent').load(this.href, function (response, status, xhr) {
                try {
                    if (status == "error") {
                        alert("Quá trình thực hiện bị lỗi, vui lòng thực hiện lại");
                    }
                    else {
                        //jQuery.noConflict();
                        $('#myModal').modal({
                            backdrop: 'static',
                            keyboard: true
                        }).modal('show');
                    }
                } catch (e) {
                }
            });
        })
    initFormModal = new FormModalDN();
    function FormModalDN() {

        this.CloseModalDN = function (IdOrClass) {
            $(IdOrClass).modal('hide');
        }
    }
});
