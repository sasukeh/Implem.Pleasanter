﻿$p.dataView = function ($control) {
    var url = $('.main-form').attr('action')
        .replace('_action_', $control.attr('data-action').toLowerCase());
    $p.ajax(url, 'post', $p.getData(), $control);
    history.pushState(null, null, url);
}