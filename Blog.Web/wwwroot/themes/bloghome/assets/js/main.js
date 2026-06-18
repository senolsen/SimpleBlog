(function () {

  "use strict";



  function onScroll() {

    document.body.classList.toggle('scrolled', window.scrollY > 50);

  }

  document.addEventListener('scroll', onScroll);

  window.addEventListener('load', onScroll);



  document.querySelectorAll('.desktop-nav .dropdown > a').forEach(link => {

    link.addEventListener('click', e => {

      if (window.innerWidth >= 1200) e.preventDefault();

    });

  });



  const drawerToggle  = document.querySelector('.mobile-drawer-toggle');

  const drawerClose   = document.querySelector('.mobile-drawer-close');

  const drawerOverlay = document.getElementById('mobileDrawerOverlay');

  const drawer        = document.getElementById('mobileDrawer');

  const body          = document.body;



  function openDrawer() {

    body.classList.add('mobile-drawer-open');

    if (drawer)        drawer.setAttribute('aria-hidden', 'false');

    if (drawerOverlay) drawerOverlay.setAttribute('aria-hidden', 'false');

    if (drawerToggle) {

      drawerToggle.setAttribute('aria-expanded', 'true');

      drawerToggle.setAttribute('aria-label', 'Menüyü kapat');

    }

    document.querySelectorAll('.mobile-drawer-section').forEach(s => {

      s.classList.add('mobile-drawer-section--collapsed');

      const btn = s.querySelector('.mobile-drawer-section-title');

      if (btn) btn.setAttribute('aria-expanded', 'false');

    });

  }



  function closeDrawer() {

    body.classList.remove('mobile-drawer-open');

    if (drawer)        drawer.setAttribute('aria-hidden', 'true');

    if (drawerOverlay) drawerOverlay.setAttribute('aria-hidden', 'true');

    if (drawerToggle) {

      drawerToggle.setAttribute('aria-expanded', 'false');

      drawerToggle.setAttribute('aria-label', 'Menüyü aç');

    }

    document.querySelectorAll('.mobile-drawer-section').forEach(s => {

      s.classList.add('mobile-drawer-section--collapsed');

      const btn = s.querySelector('.mobile-drawer-section-title');

      if (btn) btn.setAttribute('aria-expanded', 'false');

    });

  }



  if (drawerToggle)  drawerToggle.addEventListener('click', () => body.classList.contains('mobile-drawer-open') ? closeDrawer() : openDrawer());

  if (drawerClose)   drawerClose.addEventListener('click', closeDrawer);

  if (drawerOverlay) drawerOverlay.addEventListener('click', closeDrawer);



  document.addEventListener('keydown', e => {

    if (e.key === 'Escape' && body.classList.contains('mobile-drawer-open')) closeDrawer();

  });



  document.querySelectorAll('.mobile-drawer-link[href], .mobile-drawer-category').forEach(l =>

    l.addEventListener('click', closeDrawer));



  document.querySelectorAll('.mobile-drawer-section-title').forEach(btn => {

    btn.addEventListener('click', function () {

      const s = this.closest('.mobile-drawer-section');

      if (!s) return;

      s.classList.toggle('mobile-drawer-section--collapsed');

      this.setAttribute('aria-expanded',

        (!s.classList.contains('mobile-drawer-section--collapsed')).toString());

    });

  });



  const scrollTop = document.querySelector('.scroll-top');

  if (scrollTop) {

    function toggleScrollTop() {

      scrollTop.classList.toggle('active', window.scrollY > 100);

    }

    window.addEventListener('load', toggleScrollTop);

    document.addEventListener('scroll', toggleScrollTop);

    scrollTop.addEventListener('click', e => {

      e.preventDefault();

      window.scrollTo({ top: 0, behavior: 'smooth' });

    });

  }



  const searchOpenBtn  = document.getElementById('btnSearchOpen');

  const searchCloseBtn = document.getElementById('btnSearchClose');

  const searchWrap     = document.getElementById('searchWrap');



  if (searchOpenBtn && searchWrap) {

    searchOpenBtn.addEventListener('click', e => {

      e.preventDefault();

      searchWrap.classList.add('active');

      const input = searchWrap.querySelector('input[name="q"]');

      if (input) input.focus();

    });

  }



  if (searchCloseBtn && searchWrap) {

    searchCloseBtn.addEventListener('click', () => searchWrap.classList.remove('active'));

  }



  document.addEventListener('keydown', e => {

    if (e.key === 'Escape' && searchWrap && searchWrap.classList.contains('active')) {

      searchWrap.classList.remove('active');

    }

  });



})();

