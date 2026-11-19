// app.js - handles collapsible sidebar, mobile drawer, and topbar notification/profile dropdowns.
document.addEventListener('DOMContentLoaded', () => {
  const sidebar = document.getElementById('appSidebar');
  const collapseBtn = document.getElementById('collapseSidebarBtn');
  const mobileBtn = document.getElementById('mobileSidebarBtn');
  const backdrop = document.getElementById('sidebarBackdrop');

  // Restore saved desktop collapse preference
  if (localStorage.getItem('mc_sidebar_collapsed') === '1') {
    document.body.classList.add('sidebar-collapsed');
  }

  if (collapseBtn) {
    collapseBtn.addEventListener('click', () => {
      const collapsed = document.body.classList.toggle('sidebar-collapsed');
      localStorage.setItem('mc_sidebar_collapsed', collapsed ? '1' : '0');
    });
  }

  if (mobileBtn && sidebar && backdrop) {
    const closeDrawer = () => {
      sidebar.classList.remove('mobile-open');
      backdrop.classList.remove('show');
    };
    mobileBtn.addEventListener('click', () => {
      sidebar.classList.toggle('mobile-open');
      backdrop.classList.toggle('show');
    });
    backdrop.addEventListener('click', closeDrawer);
  }

  // Topbar dropdowns (notifications + profile)
  const dropdowns = document.querySelectorAll('.dropdown');
  dropdowns.forEach((dd) => {
    const trigger = dd.querySelector('[data-dropdown-trigger]');
    if (!trigger) return;
    trigger.addEventListener('click', (e) => {
      e.stopPropagation();
      const wasOpen = dd.classList.contains('open');
      dropdowns.forEach((other) => other.classList.remove('open'));
      if (!wasOpen) dd.classList.add('open');
    });
  });

  document.addEventListener('click', () => {
    dropdowns.forEach((dd) => dd.classList.remove('open'));
  });
});
