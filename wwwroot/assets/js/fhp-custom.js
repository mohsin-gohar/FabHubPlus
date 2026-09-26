(function() {
	"use strict";

	// Preloader
	const preloader = document.getElementById("preloader");
	if (preloader) {
		window.addEventListener("load", () => {
			preloader.classList.add("hidden");
		});
	}

	// ---------- Premium navbar: scroll-progress hairline + smooth anchors ----------
	(function navbarExtras() {
		const bar = document.getElementById("fhpNavProgress");
		if (bar) {
			let ticking = false;
			const update = () => {
				ticking = false;
				const doc = document.documentElement;
				const max = (doc.scrollHeight - window.innerHeight) || 1;
				const y = window.scrollY || doc.scrollTop || 0;
				const p = Math.max(0, Math.min(1, y / max));
				bar.style.transform = "scaleX(" + p.toFixed(4) + ")";
			};
			window.addEventListener("scroll", () => {
				if (!ticking) { ticking = true; requestAnimationFrame(update); }
			}, { passive: true });
			window.addEventListener("resize", update);
			update();
		}

		// Same-page anchors glide through Lenis when it is active, otherwise
		// fall back to the native smooth behaviour.
		document.addEventListener("click", (e) => {
			const a = e.target && e.target.closest ? e.target.closest('a[href^="#"]') : null;
			if (!a) return;
			const hash = a.getAttribute("href");
			if (!hash || hash === "#") return;
			let target = null;
			try { target = document.querySelector(hash); } catch (_) { return; }
			if (!target) return;
			e.preventDefault();
			if (window.fhpLenis) {
				window.fhpLenis.scrollTo(target, { offset: -110, duration: 1.2 });
			} else {
				target.scrollIntoView({ behavior: "smooth", block: "start" });
			}
			history.replaceState(null, "", hash);
		});
	})();

	// Navbar Sticky (Misao)
	const navbar = document.getElementById("navbar");
    if (navbar) {
		document.addEventListener("DOMContentLoaded", () => {
			const navbar = document.querySelector('#navbar');
			window.addEventListener('scroll', () => {
				if (window.scrollY >= 120) {
					navbar.classList.add('navbar-sticky');
				} else {
					navbar.classList.remove('navbar-sticky');
				}
			});
		});
	}

	// Search Box
	const searchBtn = document.getElementById("searchBtn");
	const searchBox = document.getElementById("searchBox");
	if (searchBtn && searchBox) {
		searchBtn.addEventListener("click", () => {
			searchBtn.classList.toggle("active");
			searchBox.classList.toggle("active");
		});
	}

	// Menu Toggle Button
	document.addEventListener("DOMContentLoaded", () => {
		const toggles = document.querySelectorAll(".navbar-burger-toggle");
		if (!toggles.length) return;
		const menu = document.querySelector(".sidebar-modal");
		if (!menu) return;
		const backdrop = document.querySelector(".backdrop");
		if (!backdrop) return;
		const closeBtn = menu.querySelector("button");
		const openMenu = () => {
			if (!menu || !backdrop) return;
			menu.classList.add("show");
			backdrop.classList.add("show");
			toggles.forEach(b => {
				if (!b) return;
				b.classList.add("is-open");
				b.setAttribute("aria-expanded", "true");
			});
		};
		const closeMenu = () => {
			if (!menu || !backdrop) return;
			menu.classList.remove("show");
			backdrop.classList.remove("show");
			toggles.forEach(b => {
				if (!b) return;
				b.classList.remove("is-open");
				b.setAttribute("aria-expanded", "false");
			});
		};
		toggles.forEach(btn => {
			if (!btn) return;
			btn.addEventListener("click", openMenu);
		});
		if (closeBtn) closeBtn.addEventListener("click", closeMenu);
		if (backdrop) backdrop.addEventListener("click", closeMenu);
	});

	// ScrollCue
	if (typeof scrollCue !== "undefined") {
		scrollCue.init();
	}

	// Swiper Sliders
	// Every slider is initialised only when its element exists, so one page can
	// never break another page (the theme's custom.js called them unguarded).
	if (typeof Swiper !== "undefined") {
		const exists = (selector) => document.querySelector(selector) !== null;

		// Hero banner + its thumbnail slider
		if (exists(".bannerSwiper")) {
			let bannerThumbs = null;
			if (exists(".bannerSwiperThumbs")) {
				bannerThumbs = new Swiper(".bannerSwiperThumbs", {
					loop: true,
					freeMode: true,
					slidesPerView: 2,
					spaceBetween: 20,
					watchSlidesProgress: true
				});
			}
			const bannerOptions = {
				loop: true,
				parallax: true,
				effect: "fade",
				autoHeight: true,
				slidesPerView: 1,
				fadeEffect: { crossFade: true },
				autoplay: { delay: 3500, disableOnInteraction: false }
			};
			if (bannerThumbs) {
				bannerOptions.thumbs = { swiper: bannerThumbs };
			}
			new Swiper(".bannerSwiper", bannerOptions);
		}

		// Live shows / upcoming events strip
		if (exists(".liveShowsSwiper")) {
			new Swiper(".liveShowsSwiper", {
				loop: true,
				slidesPerView: 1,
				spaceBetween: 25,
				autoplay: { delay: 3500, disableOnInteraction: false },
				breakpoints: {
					640: { slidesPerView: 2 },
					720: { slidesPerView: 2 },
					1024: { slidesPerView: 3 },
					1280: { slidesPerView: 4 },
					1536: { slidesPerView: 5 }
				}
			});
		}

		// Fan feedback / testimonials
		if (exists(".feedbackSwiper")) {
			new Swiper(".feedbackSwiper", {
				loop: true,
				slidesPerView: 1,
				spaceBetween: 25,
				autoplay: { delay: 3500, disableOnInteraction: false },
				navigation: {
					nextEl: ".feedback-button-next",
					prevEl: ".feedback-button-prev"
				},
				breakpoints: {
					1280: { slidesPerView: 2, spaceBetween: 140 }
				}
			});
		}
	}

	// Counter
	if ("IntersectionObserver" in window) {
        let counterObserver = new IntersectionObserver(function (entries, observer) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                let counter = entry.target;
                let target = parseInt(counter.innerText, 10);
                let step = target / 200;
                let current = 0;
                let timer = setInterval(function () {
                    current += step;
                    counter.innerText = Math.floor(current);
                    if (parseInt(counter.innerText, 10) >= target) {
                    clearInterval(timer);
                    }
                }, 10);
                counterObserver.unobserve(counter);
                }
            });
        });
        let counters = document.querySelectorAll(".counter");
            counters.forEach(function (counter) {
            counterObserver.observe(counter);
        });
    }

	// Quantity Counter
	document.querySelectorAll(".qty-counter-input").forEach(counter => {
		const input = counter.querySelector("input[type='number']");
		const minusBtn = counter.querySelector(".qty-minus");
		const plusBtn = counter.querySelector(".qty-plus");
		plusBtn.addEventListener("click", () => {
			const current = parseInt(input.value, 10) || 0;
			input.value = current + 1;
		});
		minusBtn.addEventListener("click", () => {
			const min = parseInt(input.min, 10) || 0;
			const current = parseInt(input.value, 10) || 0;
			if (current > min) {
				input.value = current - 1;
			}
		});
	});

	// Accordion
	// Supports both the component markup (.fhp-accordion__*) and the original
	// Misao markup (.accordion-*), so any page can use either vocabulary.
	const accordion = document.getElementById("accordion");
	if (accordion) {
		const items = accordion.querySelectorAll(".accordion-item, .fhp-accordion__item");
		const panelOf = (item) => item.querySelector(".accordion-panel, .fhp-accordion__panel");
		const setOpen = (item, open) => {
			item.classList.toggle("active", open);
			item.classList.toggle("is-active", open);
			const panel = panelOf(item);
			if (panel) {
				panel.classList.toggle("hidden", !open);
				panel.classList.toggle("is-hidden", !open);
			}
		};
		items.forEach(item => {
			const toggle = item.querySelector(".accordion-toggle, .fhp-accordion__btn");
			if (!toggle) return;
			toggle.addEventListener("click", () => {
				const willOpen = !item.classList.contains("is-active") && !item.classList.contains("active");
				items.forEach(i => setOpen(i, false));
				if (willOpen) setOpen(item, true);
			});
		});
		// Open the first entry by default so the panel never looks empty.
		if (items.length) setOpen(items[0], true);
	}

	// Tabs
	document.querySelectorAll(".tabs").forEach((tabsBlock) => {
		const navLinks = tabsBlock.querySelectorAll(".nav-link");
		const tabPanes = tabsBlock.querySelectorAll(".tab-pane");
		navLinks.forEach((btn, index) => {
			btn.addEventListener("click", () => {
				// Remove active from current tab group only
				navLinks.forEach(link => link.classList.remove("active"));
				tabPanes.forEach(pane => pane.classList.remove("active"));
				// Activate clicked tab + its pane
				btn.classList.add("active");
				tabPanes[index].classList.add("active");
			});
		});
	});

	// LTR/RTL Toggle
	const rtlToggleBtn = document.getElementById("ltrRtlToggle");
	if (rtlToggleBtn) {
		const htmlTag = document.documentElement;
		const icon = rtlToggleBtn.querySelector("i");
		// Load direction from storage
		const savedDirection = localStorage.getItem("textDirection") || "ltr";
		htmlTag.setAttribute("dir", savedDirection);
		// Set correct icon on load
		if (savedDirection === "rtl") {
			icon.className = "ri-text-direction-l";
		} else {
			icon.className = "ri-text-direction-r";
		}
		// Toggle direction on click
		rtlToggleBtn.addEventListener("click", () => {
			const current = htmlTag.getAttribute("dir");
			const newDir = current === "ltr" ? "rtl" : "ltr";
			// Update direction
			htmlTag.setAttribute("dir", newDir);
			localStorage.setItem("textDirection", newDir);
			// Swap icon
			if (newDir === "rtl") {
				icon.className = "ri-text-direction-l"; // RTL icon
			} else {
				icon.className = "ri-text-direction-r"; // LTR icon
			}
		});
	}

	// Back to Top
    const backToTopBtn = document.getElementById("backToTopBtn");
    if (backToTopBtn) {
        const backToTopBtn = document.getElementById("backToTopBtn");
        window.addEventListener("scroll", () => {
            if (window.scrollY > 300) {
                backToTopBtn.classList.add("show");
            } else {
                backToTopBtn.classList.remove("show");
            }
        });
        backToTopBtn.addEventListener("click", () => {
            if (window.fhpLenis) {
                window.fhpLenis.scrollTo(0, { duration: 1.1 });
            } else {
                window.scrollTo({ top: 0, behavior: 'smooth' });
            }
        });
    }
    
})();

// Sidebar Navbar Menu
const sidebar = document.querySelector('.sidebar-navbar-nav');
if (sidebar) {
    const list = sidebar.querySelectorAll('.nav-item');
    function accordion(e) {
        e.stopPropagation();
        if (this.classList.contains('active')) {
            this.classList.remove('active');
        } else if (this.parentElement.parentElement.classList.contains('active')) {
            this.classList.add('active');
        } else {
            for (let i = 0; i < list.length; i++) {
                list[i].classList.remove('active');
            }
            this.classList.add('active');
        }
    }
    for (let i = 0; i < list.length; i++) {
        list[i].addEventListener('click', accordion);
    }
}