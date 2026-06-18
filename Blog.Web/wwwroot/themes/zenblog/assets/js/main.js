/**
* Template Name: ZenBlog
* Template URL: https://bootstrapmade.com/zenblog-bootstrap-blog-template/
* Updated: Aug 08 2024 with Bootstrap v5.3.3
* Author: BootstrapMade.com
* License: https://bootstrapmade.com/license/
*/

(function() {
  "use strict";

  function toggleScrolled() {
    const selectBody = document.querySelector('body');
    const selectHeader = document.querySelector('#header');
    if (!selectHeader) return;
    if (!selectHeader.classList.contains('scroll-up-sticky') && !selectHeader.classList.contains('sticky-top') && !selectHeader.classList.contains('fixed-top')) return;
    window.scrollY > 50 ? selectBody.classList.add('scrolled') : selectBody.classList.remove('scrolled');
  }

  document.addEventListener('scroll', toggleScrolled);
  window.addEventListener('load', toggleScrolled);

  /* Desktop dropdown — sadece hover ile, tıklama engellenmesin */
  const desktopDropdowns = document.querySelectorAll('.desktop-nav .dropdown > a');
  desktopDropdowns.forEach(link => {
    link.addEventListener('click', function(e) {
      if (window.innerWidth >= 1200) {
        e.preventDefault();
      }
    });
  });

  /* Mobile slide-in drawer */
  const drawerToggle = document.querySelector('.mobile-drawer-toggle');
  const drawerClose = document.querySelector('.mobile-drawer-close');
  const drawerOverlay = document.getElementById('mobileDrawerOverlay');
  const drawer = document.getElementById('mobileDrawer');
  const body = document.body;

  function openDrawer() {
    body.classList.add('mobile-drawer-open');
    if (drawer) drawer.setAttribute('aria-hidden', 'false');
    if (drawerOverlay) drawerOverlay.setAttribute('aria-hidden', 'false');
    if (drawerToggle) {
      drawerToggle.setAttribute('aria-expanded', 'true');
      drawerToggle.setAttribute('aria-label', 'Menüyü kapat');
    }

    // Mobil menü açılırken kategoriler paneli kapalı başlasın.
    // Kullanıcı daha önce açıp kapatmış olsa bile, tekrar açıldığında
    // varsayılan state "collapsed" olsun.
    document.querySelectorAll('.mobile-drawer-section').forEach(section => {
      section.classList.add('mobile-drawer-section--collapsed');
      const toggle = section.querySelector('.mobile-drawer-section-title');
      if (toggle) toggle.setAttribute('aria-expanded', 'false');
    });
  }

  function closeDrawer() {
    body.classList.remove('mobile-drawer-open');
    if (drawer) drawer.setAttribute('aria-hidden', 'true');
    if (drawerOverlay) drawerOverlay.setAttribute('aria-hidden', 'true');
    if (drawerToggle) {
      drawerToggle.setAttribute('aria-expanded', 'false');
      drawerToggle.setAttribute('aria-label', 'Menüyü aç');
    }
    document.querySelectorAll('.mobile-drawer-section').forEach(section => {
      section.classList.add('mobile-drawer-section--collapsed');
      const toggle = section.querySelector('.mobile-drawer-section-title');
      if (toggle) toggle.setAttribute('aria-expanded', 'false');
    });
  }

  function toggleDrawer() {
    body.classList.contains('mobile-drawer-open') ? closeDrawer() : openDrawer();
  }

  if (drawerToggle) {
    drawerToggle.addEventListener('click', toggleDrawer);
  }

  if (drawerClose) {
    drawerClose.addEventListener('click', closeDrawer);
  }

  if (drawerOverlay) {
    drawerOverlay.addEventListener('click', closeDrawer);
  }

  document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape' && body.classList.contains('mobile-drawer-open')) {
      closeDrawer();
    }
  });

  document.querySelectorAll('.mobile-drawer-link[href]').forEach(link => {
    link.addEventListener('click', closeDrawer);
  });

  document.querySelectorAll('.mobile-drawer-category').forEach(link => {
    link.addEventListener('click', closeDrawer);
  });

  document.querySelectorAll('.mobile-drawer-section-title').forEach(button => {
    button.addEventListener('click', function() {
      const section = this.closest('.mobile-drawer-section');
      if (!section) return;
      section.classList.toggle('mobile-drawer-section--collapsed');
      const expanded = !section.classList.contains('mobile-drawer-section--collapsed');
      this.setAttribute('aria-expanded', expanded.toString());
    });
  });

  const preloader = document.querySelector('#preloader');
  if (preloader) {
    window.addEventListener('load', () => {
      preloader.remove();
    });
  }

  let scrollTop = document.querySelector('.scroll-top');

  function toggleScrollTop() {
    if (scrollTop) {
      window.scrollY > 100 ? scrollTop.classList.add('active') : scrollTop.classList.remove('active');
    }
  }

  if (scrollTop) {
    scrollTop.addEventListener('click', (e) => {
      e.preventDefault();
      window.scrollTo({
        top: 0,
        behavior: 'smooth'
      });
    });
  }

  window.addEventListener('load', toggleScrollTop);
  document.addEventListener('scroll', toggleScrollTop);

  const searchOpenBtn = document.getElementById('btnSearchOpen');
  const searchCloseBtn = document.getElementById('btnSearchClose');
  const searchWrap = document.getElementById('searchWrap');

  if (searchOpenBtn && searchWrap) {
    searchOpenBtn.addEventListener('click', function(e) {
      e.preventDefault();
      searchWrap.classList.add('active');
      const input = searchWrap.querySelector('input[name="q"]');
      if (input) input.focus();
    });
  }

  if (searchCloseBtn && searchWrap) {
    searchCloseBtn.addEventListener('click', function() {
      searchWrap.classList.remove('active');
    });
  }

  function aosInit() {
    if (typeof AOS !== 'undefined') {
      AOS.init({
        duration: 600,
        easing: 'ease-in-out',
        once: true,
        mirror: false
      });
    }
  }
  window.addEventListener('load', aosInit);

  function initSwiper() {
    document.querySelectorAll(".init-swiper").forEach(function(swiperElement) {
      let config = JSON.parse(
        swiperElement.querySelector(".swiper-config").innerHTML.trim()
      );

      if (swiperElement.classList.contains("swiper-tab")) {
        initSwiperWithCustomPagination(swiperElement, config);
      } else {
        new Swiper(swiperElement, config);
      }
    });
  }

  window.addEventListener("load", initSwiper);

})();
