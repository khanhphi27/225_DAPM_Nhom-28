/* =============================================
   QLTB - Notifications Page Scripts
   ============================================= */

document.addEventListener('DOMContentLoaded', function () {
    // Click vào thông báo -> đánh dấu đã đọc
    document.querySelectorAll('.notif-item').forEach(function (item) {
        item.addEventListener('click', function (e) {
            if (e.target.tagName === 'BUTTON') return;
            if (this.classList.contains('unread')) {
                this.classList.remove('unread');
                var dot = this.querySelector('.notif-dot');
                if (dot) dot.remove();
                updateCounts();
            }
        });
    });
});

function filterNotif(type, btn) {
    document.querySelectorAll('.filter-btn').forEach(function (b) {
        b.classList.remove('active');
    });
    btn.classList.add('active');

    var visible = 0;
    document.querySelectorAll('.notif-item[data-type]').forEach(function (item) {
        var show = type === 'all'
            || item.dataset.type === type
            || (type === 'unread' && item.classList.contains('unread'));
        item.style.display = show ? '' : 'none';
        if (show) visible++;
    });

    document.getElementById('emptyState').classList.toggle('d-none', visible > 0);
}

function markAllRead() {
    document.querySelectorAll('.notif-item.unread').forEach(function (item) {
        item.classList.remove('unread');
        var dot = item.querySelector('.notif-dot');
        if (dot) dot.remove();
    });
    updateCounts();
}

function deleteNotif(btn) {
    var item = btn.closest('.notif-item');
    item.style.transition = 'opacity 0.3s';
    item.style.opacity = '0';
    setTimeout(function () {
        item.remove();
        updateCounts();
        var visible = document.querySelectorAll('.notif-item[data-type]').length;
        document.getElementById('emptyState').classList.toggle('d-none', visible > 0);
    }, 300);
}

function clearAll() {
    if (!confirm('Xóa tất cả thông báo?')) return;
    document.querySelectorAll('.notif-item[data-type]').forEach(function (item) {
        item.remove();
    });
    document.getElementById('emptyState').classList.remove('d-none');
    document.getElementById('totalCount').textContent = '0';
    document.getElementById('unreadCount').textContent = '0';
}

function updateCounts() {
    var unread = document.querySelectorAll('.notif-item.unread').length;
    var total  = document.querySelectorAll('.notif-item[data-type]').length;
    document.getElementById('unreadCount').textContent = unread;
    document.getElementById('totalCount').textContent  = total;
}
