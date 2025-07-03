var loadpage = {
    reload: function (idAppndLoad, textalert) {
        $(`${idAppndLoad}`).empty().append(`<div class="tab-content"><div id="loadingSpinner" class="text-center my-5"><div class="spinner-border text-primary" role="status"></div><div class="mt-2">${textalert}</div></div></div>`)
    }
};