/* =============================================
   QLTB - Profile Page Scripts
   ============================================= */

function togglePwd(id, btn) {
    var input = document.getElementById(id);
    if (input.type === 'password') {
        input.type = 'text';
        btn.textContent = '🙈';
    } else {
        input.type = 'password';
        btn.textContent = '👁';
    }
}

function checkStrength(val) {
    var bar  = document.getElementById('strengthBar');
    var text = document.getElementById('strengthText');

    if (val.length === 0) {
        bar.style.width = '0%';
        text.textContent = '';
        return;
    }

    if (val.length < 6) {
        bar.style.width = '25%';
        bar.className = 'progress-bar bg-danger';
        text.textContent = 'Yếu';
        text.className = 'text-danger';
    } else if (val.length < 10) {
        bar.style.width = '60%';
        bar.className = 'progress-bar bg-warning';
        text.textContent = 'Trung bình';
        text.className = 'text-warning';
    } else {
        bar.style.width = '100%';
        bar.className = 'progress-bar bg-success';
        text.textContent = 'Mạnh';
        text.className = 'text-success';
    }
}

function submitChangePassword() {
    var cur = document.getElementById('currentPwd').value;
    var nw  = document.getElementById('newPwd').value;
    var cf  = document.getElementById('confirmPwd').value;

    if (!cur || !nw || !cf) {
        showToast('Vui lòng điền đầy đủ thông tin!', 'danger');
        return;
    }
    if (nw.length < 6) {
        showToast('Mật khẩu mới phải có ít nhất 6 ký tự!', 'danger');
        return;
    }
    if (nw !== cf) {
        showToast('Mật khẩu xác nhận không khớp!', 'danger');
        return;
    }

    showToast('Đổi mật khẩu thành công!', 'success');
    resetPasswordForm();
}

function resetPasswordForm() {
    document.getElementById('passwordForm').reset();
    document.getElementById('strengthBar').style.width = '0%';
    document.getElementById('strengthText').textContent = '';
    document.getElementById('matchMsg').textContent = '';
}

function showToast(msg, type) {
    var toast = document.getElementById('toastMsg');
    toast.className = 'toast align-items-center text-white border-0 bg-' + type;
    document.getElementById('toastText').textContent = msg;
    new bootstrap.Toast(toast, { delay: 3000 }).show();
}

document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('confirmPwd').addEventListener('input', function () {
        var msg = document.getElementById('matchMsg');
        var match = this.value === document.getElementById('newPwd').value;
        msg.textContent = match ? '✅ Mật khẩu khớp' : '❌ Mật khẩu không khớp';
        msg.className   = match ? 'text-success' : 'text-danger';
    });
});
