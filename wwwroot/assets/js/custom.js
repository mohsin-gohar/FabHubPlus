(function() {
	"use strict";

	// Preloader
	const preloader = document.getElementById("preloader");
	if (preloader) {
		window.addEventListener("load", () => {
			preloader.classList.add("hidden");
		});
	}

	// Navbar Sticky
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
		};
		const closeMenu = () => {
			if (!menu || !backdrop) return;
			menu.classList.remove("show");
			backdrop.classList.remove("show");
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

	// Swiper Slider
	if (typeof Swiper !== "undefined") {

		// Banner Slider
		var swiper = new Swiper(".bannerSwiperThumbs", {
			loop: true,
			freeMode: true,
			slidesPerView: 2,
			spaceBetween: 20,
			watchSlidesProgress: true
		});
		var swiper = new Swiper(".bannerSwiper", {
			loop: true,
			parallax: true,
			effect: "fade",
			autoHeight: true,
			slidesPerView: 1,
			fadeEffect: {
				crossFade: true
			},
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			thumbs: {
				swiper: swiper
			}
		});

		// Banner Slider 2
		var swiper = new Swiper(".bannerSwiper2", {
			loop: true,
			parallax: true,
			effect: "fade",
			autoHeight: true,
			slidesPerView: 1,
			fadeEffect: {
				crossFade: true
			},
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			pagination: {
				el: ".banner-swiper-pagination",
				clickable: true,
				renderBullet: function (index, className) {
					const number = index + 1;
					const formatted = number < 10 ? `0${number}` : number;
					return `<span class="${className}">${formatted}</span>`;
				}
			}
		});

		// Banner Slider 3
		var swiper = new Swiper(".bannerSwiper3", {
			loop: true,
			speed: 3000,
			slidesPerView: 2,
			spaceBetween: 15,
			autoplay: {
				delay: 0,
				disableOnInteraction: false
			},
			breakpoints: {
				640: {
					slidesPerView: 3
				},
				720: {
					slidesPerView: 4
				},
				1024: {
					slidesPerView: 5
				},
				1280: {
					slidesPerView: 6
				}
			}
		});

		// Banner Slider 4
		var swiper = new Swiper(".bannerSwiper4", {
			rtl: true,
			loop: true,
			speed: 3000,
			slidesPerView: 2,
			spaceBetween: 15,
			autoplay: {
				delay: 0,
				disableOnInteraction: false
			},
			breakpoints: {
				640: {
					slidesPerView: 3
				},
				720: {
					slidesPerView: 4
				},
				1024: {
					slidesPerView: 5
				},
				1280: {
					slidesPerView: 6
				}
			}
		});

		// Banner Slider 5
		var swiper = new Swiper(".bannerSwiper5", {
			loop: true,
			grabCursor: true,
			slidesPerView: "2",
			grabCursor: false,
			effect: "coverflow",
			simulateTouch: false,
			coverflowEffect: {
				rotate: 50,
				stretch: 0,
				depth: 100,
				modifier: 1,
				slideShadows: true
			},
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			breakpoints: {
				720: {
					slidesPerView: 3
				},
				1024: {
					slidesPerView: 3
				},
				1280: {
					slidesPerView: 3
				}
			},
			navigation: {
				nextEl: ".banner-button-next",
				prevEl: ".banner-button-prev"
			}
		});
		var swiper = new Swiper(".bannerSwiper5Content", {
			loop: true,
			slidesPerView: 1,
			grabCursor: false,
			simulateTouch: false,
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			navigation: {
				nextEl: ".banner-button-next",
				prevEl: ".banner-button-prev"
			}
		});

		// Live Shows Slider
		var swiper = new Swiper(".liveShowsSwiper", {
			loop: true,
			slidesPerView: 2,
			spaceBetween: 25,
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			breakpoints: {
				640: {
					slidesPerView: 3
				},
				720: {
					slidesPerView: 2
				},
				1024: {
					slidesPerView: 3
				},
				1280: {
					slidesPerView: 4
				},
				1536: {
					slidesPerView: 5
				}
			}
		});

		// Feedback Slider
		var swiper = new Swiper(".feedbackSwiper", {
			loop: true,
			slidesPerView: 1,
			spaceBetween: 25,
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			breakpoints: {
				720: {
					slidesPerView: 1
				},
				1024: {
					slidesPerView: 1
				},
				1280: {
					spaceBetween: 140,
					slidesPerView: 2
				}
			},
			navigation: {
				nextEl: ".feedback-button-next",
				prevEl: ".feedback-button-prev"
			}
		});

		// Feedback Slider 2
		var swiper = new Swiper(".feedbackSwiper2", {
			loop: true,
			slidesPerView: 1,
			spaceBetween: 25,
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			navigation: {
				nextEl: ".feedback-button-next",
				prevEl: ".feedback-button-prev"
			}
		});

		// Misao Originals Slider
		var swiper = new Swiper(".misaoOriginalsSwiper", {
			loop: true,
			slidesPerView: 2,
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			breakpoints: {
				640: {
					slidesPerView: 3
				},
				720: {
					slidesPerView: 4
				}
			}
		});

		// Movies Slider
		var swiper = new Swiper(".moviesSwiper", {
			loop: true,
			slidesPerView: 1,
			spaceBetween: 25,
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			breakpoints: {
				640: {
					slidesPerView: 2
				},
				720: {
					slidesPerView: 3
				},
				1024: {
					slidesPerView: 4
				},
				1280: {
					slidesPerView: 5
				},
				1536: {
					slidesPerView: 6
				}
			}
		});

		// Movies Slider 2
		var swiper = new Swiper(".moviesSwiper2", {
			loop: true,
			slidesPerView: 1,
			spaceBetween: 25,
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			breakpoints: {
				640: {
					slidesPerView: 2
				},
				720: {
					slidesPerView: 3
				},
				1024: {
					slidesPerView: 4
				},
				1280: {
					slidesPerView: 5
				}
			}
		});

		// Movies Slider 3
		var swiper = new Swiper(".moviesSwiper3", {
			loop: true,
			slidesPerView: 1,
			spaceBetween: 25,
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			breakpoints: {
				640: {
					slidesPerView: 1
				},
				720: {
					slidesPerView: 2
				},
				1024: {
					slidesPerView: 3
				},
				1280: {
					slidesPerView: 4
				}
			}
		});

		// Featured Movies Slider
		var swiper = new Swiper(".featuredMoviesSwiper", {
			loop: true,
			effect: "fade",
			parallax: true,
			slidesPerView: 1,
			fadeEffect: {
				crossFade: true
			},
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			navigation: {
				nextEl: ".featured-movies-button-next",
				prevEl: ".featured-movies-button-prev"
			}
		});

		// Testimonials Slider
		var swiper = new Swiper(".testimonialsSwiper", {
			loop: true,
			slidesPerView: 1,
			spaceBetween: 25,
			autoplay: {
				delay: 3500,
				disableOnInteraction: false
			},
			breakpoints: {
				640: {
					slidesPerView: 2
				},
				1024: {
					slidesPerView: 3
				},
				1280: {
					slidesPerView: 4
				}
			}
		});

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
	const accordion = document.getElementById("accordion");
	if (accordion) {
		const items = accordion.querySelectorAll(".accordion-item");
		items.forEach(item => {
			const toggle = item.querySelector(".accordion-toggle");
			toggle.addEventListener("click", () => {
				// Close all items
				items.forEach(i => {
					i.classList.remove("active");
					i.querySelector(".accordion-panel").classList.add("hidden");
				});
				// Open the clicked item
				item.classList.add("active");
				item.querySelector(".accordion-panel").classList.remove("hidden");
			});
		});
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
            window.scrollTo({ top: 0, behavior: 'smooth' });
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