var partnerMana = {
    getUrl : function () {
        var areas = window.location.pathname.split('/')[1];
        var username = window.location.pathname.split('/')[2];
        var controller = window.location.pathname.split('/')[3];
        return `/${areas}/${username}/${controller}`;
    }
}