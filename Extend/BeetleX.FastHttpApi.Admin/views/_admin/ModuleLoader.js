function moduleLoad(url) {
    $.get(url, function (result) {
        var html = $(result);
        var __templates = html;
        $("[slot]").each(function () {
            var id = $(this).attr('slot');
            var body = $(__templates).find('#' + id).html();
            $(this).html(body);
        });
    });
}
$(document).ready(function () {
    moduleLoad("/_admin/PublicModule.html");
});