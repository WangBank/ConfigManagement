function ajax_delete(url,data) {
    $.ajax({
        type: 'post',
        url: url,
        cache: false,
        async: false,
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (result) {
            if (result.code == "0") {
                parent.layer.msg("操作成功");
                window.location.reload();
            } else if (result.code == "-2") {
                layer.msg(result.msg);
                parent.parent.parent.window.location.href = '/Login';
            }
            else {
                layer.msg(result.msg);
            }
        },
        error: function (msg) {
            layer.msg("系统异常");
        }
    });
}


function ajax_exec(url,data) {
  $.ajax({
      type: 'post',
      url: url,
      cache: false,
      async: true,
      contentType: 'application/json',
      data: JSON.stringify(data),
      success: function (result) {
          if (result.code == "0") {
              parent.layer.msg("操作成功");
              window.location.reload();
          } else if (result.code == "-2") {
              layer.msg(result.msg);
              parent.parent.parent.window.location.href = '/Login';
          }
          else {
              layer.msg(result.msg);
          }
      },
      error: function (msg) {
          layer.msg("系统异常");
      }
  });
}

function ajax_post(url,data) {
    $.ajax({
        type: 'post',
        url: url,
        cache: false,
        async: false,
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (result) {
            if (result.code == "0") {
                parent.layer.msg("操作成功");
                var iframeIndex = parent.layer.getFrameIndex(window.name);
                parent.layer.close(iframeIndex);
                parent.window.location.reload();
                return true;
            } else if (result.code == "-2") {
                layer.msg(result.msg);
                parent.parent.parent.window.location.href = '/Login';
            }
            else {
                layer.msg(result.msg);
                return false;
            }
        },
        error: function (msg) {
            layer.msg("系统异常");
            return false;
        }
    });
}

function ajax_update_post(url, data) {
    $.ajax({
        type: 'post',
        url: url,
        cache: false,
        async: false,
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (result) {
            if (result.code == "0") {
                parent.layer.msg("操作成功");
                window.location.reload();
                return true;
            } else if (result.code == "-2") {
                layer.msg(result.msg);
                parent.parent.parent.window.location.href = '/Login';
            }
            else {
                layer.msg(result.msg);
                return false;
            }
        },
        error: function (msg) {
            layer.msg("系统异常");
            return false;
        }
    });
}

/**
 * 对无参数的操作
 * @param {any} url
 */
function ajax_emptypara_delete(url) {
    var flag = false;//声明一个变量
    $.ajax({
        type: 'post',
        url: url,
        async: false,
        success: function (result) {
            if (result.code == "0") {
                parent.layer.msg("操作成功");
                flag = true;
            } else if (result.code == "-2") {
                layer.msg(result.msg);
                parent.parent.parent.window.location.href = '/Login';
                flag = false;
            }
            else {
                layer.msg(result.msg);
                flag = false;
            }
        },
        error: function (errorinfo) {
            var errordata = JSON.parse(errorinfo.responseText)
            layer.msg(errordata.msg, { icon: 2, time: 5000 });
            flag = false;
        }
    });
    return flag;
}

function ajax_form_noRefreshPost(url, data) {
    var flag = false;//声明一个变量
    $.ajax({
        type: 'post',
        url: url,
        cache: false,
        async: false,
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (result) {
            if (result.code == "0") {
                parent.layer.msg("操作成功");
                var iframeIndex = parent.layer.getFrameIndex(window.name);
                parent.layer.close(iframeIndex);
                //parent.window.location.reload();
                flag = true;
            } else if (result.code == "-2") {
                layer.msg(result.msg);
                parent.parent.parent.window.location.href = '/Login';
                flag = false;
            }
            else {
                layer.msg(result.msg);
                flag = false;
            }
        },
        error: function (errorinfo) {
            var errordata = JSON.parse(errorinfo.responseText)
            layer.msg(errordata.msg, { icon: 2, time: 5000 });
            flag = false;
        }
    });
    return flag;
}
