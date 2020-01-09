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
 __footer += '    <div class="navbar navbar-inverse navbar-fixed-bottom" style="height:30px;background-color:#e4e4e4;border-color:#fff;padding-top:6px;text-align:center;">';
 __footer += '        <div class="container">';
 __footer += '            <form v-if="!status" class="form-inline">';
 __footer += '                <div class="form-group">';
 __footer += '                    <label for="exampleInputName2">Name</label>';
 __footer += '                    <input type="text" v-model="nickName" class="form-control">';
 __footer += '                </div>';
 __footer += '                <button type="button" class="btn btn-default" @click="onLogin">Login</button>';
 __footer += '            </form>';
 __footer += '            <form v-else class="form-inline">';
 __footer += '                <div class="form-group">';
 __footer += '                    <div class="dropup">';
 __footer += '                        <button class="btn btn-default dropdown-toggle" type="button" id="dropdownMenu2" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
 __footer += '                            {{roomName}}';
 __footer += '                            <span class="caret"></span>';
 __footer += '                        </button>';
 __footer += '                        <ul class="dropdown-menu" aria-labelledby="dropdownMenu2">';
 __footer += '                            <li v-for="item in rooms"><a href="javascript:void(0)" @click="onSelectRoom(item.Name)">{{item.Name}}</a></li>';
 __footer += '                           ';
 __footer += '                        </ul>';
 __footer += '                    </div>';
 __footer += '                </div>';
 __footer += '                <div v-if="roomName!=\'Rooms\'" class="form-group">';
 __footer += '';
 __footer += '                    <input type="text" v-model="message" class="form-control" style="width:500px;">';
 __footer += '                </div>';
 __footer += '                <button v-if="roomName!=\'Rooms\'" type="button" class="btn btn-default" @click="onTalk">Talk</button>';
 __footer += '            </form>';
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
        props: ['status','rooms'],
        data: function () {
            return {
                count: 0,
                roomName: 'Rooms',
                message: '',
                nickName:''
            }
        },
        methods: {
            onSelectRoom: function (name) {
                this.roomName = name;

                this.$emit('select', name)
            },
            onTalk: function () {
                this.$emit('talk', this.message);
                this.message = '';
            },
            onLogin: function () {
                if (!this.nickName) {
                    alert('enter you name!');
                    return;
                }
                this.$emit('login', this.nickName);
            }
        },
        template: __footer,
    })

