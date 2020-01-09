/*
* Generate js with vuejs Copyright © ikende.com 2019 email:henryfan@msn.com 
*/
var __header="";
 __header += '    <div class="navbar navbar-inverse navbar-fixed-top">';
 __header += '        <div class="container">';
 __header += '            <div class="navbar-header">';
 __header += '                <button class="navbar-toggle collapsed" type="button" data-toggle="collapse" data-target=".navbar-collapse">';
 __header += '                    <span class="sr-only">Toggle navigation</span>';
 __header += '                    <span class="icon-bar"></span>';
 __header += '                    <span class="icon-bar"></span>';
 __header += '                    <span class="icon-bar"></span>';
 __header += '                </button>';
 __header += '                <a class="navbar-brand hidden-sm" href="http://ikende.com" target="_blank">Beetlex FathttpApi 示例</a>';
 __header += '            </div>';
 __header += '            <div class="navbar-collapse collapse" role="navigation">';
 __header += '                <ul class="nav navbar-nav">';
 __header += '                   ';
 __header += '                </ul>';
 __header += '                <ul class="nav navbar-nav navbar-right hidden-sm">';
 __header += '                    <li><a href="http://ikende.com" target="_blank">关于</a></li>';
 __header += '                </ul>';
 __header += '            </div>';
 __header += '        </div>';
 __header += '    </div>';
var __footer="";
 __footer += '    <div class="navbar navbar-inverse navbar-fixed-bottom" style="height:30px;">';
 __footer += '        <div class="container">';
 __footer += '           ';
 __footer += '        </div>';
 __footer += '    </div>';


    Vue.component('page-header', {
        props: ['info', 'page'],
        data: function () {
            return {
                count: 0
            }
        },
        template: __header
    });

    Vue.component('page-footer', {
        props: ['info'],
        data: function () {
            return {
                count: 0
            }
        },
        template: __footer,
    })

