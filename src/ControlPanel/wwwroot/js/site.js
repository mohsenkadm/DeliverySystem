// ════════════════════════════════════════════════════════════════
// نظام إدارة التوصيل — Site JS
// ════════════════════════════════════════════════════════════════

// ─── Auto-dismiss alerts after 4 seconds ─────────────────────
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.alert-dismissible').forEach(alert => {
        setTimeout(() => {
            const btn = alert.querySelector('.btn-close');
            if (btn) btn.click();
        }, 4000);
    });

    // ─── Confirm delete forms ─────────────────────────────────
    document.querySelectorAll('form[data-confirm]').forEach(form => {
        form.addEventListener('submit', e => {
            if (!confirm(form.dataset.confirm)) e.preventDefault();
        });
    });

    // ─── Mobile sidebar overlay ───────────────────────────────
    const sidebar = document.getElementById('sidebar');
    if (sidebar && window.innerWidth <= 768) {
        document.getElementById('sidebarToggle')?.addEventListener('click', () => {
            sidebar.classList.toggle('mobile-open');
        });
        document.addEventListener('click', e => {
            if (!sidebar.contains(e.target) && !e.target.closest('#sidebarToggle')) {
                sidebar.classList.remove('mobile-open');
            }
        });
    }
});

// ─── Number formatting helper ─────────────────────────────────
function formatNumber(n) {
    return new Intl.NumberFormat('ar-SA', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n);
}
