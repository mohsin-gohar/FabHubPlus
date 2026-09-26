(function() {
	"use strict";

	// Register GSAP plugins once
	gsap.registerPlugin(ScrollTrigger, SplitText);

	// Split text animation
	function initSplitTextAnimation() {
		const targets = gsap.utils.toArray(".text-animation, .text_animation");
		targets.forEach((el) => {
			// Split into lines (you can add "words, chars" if needed)
			const split = new SplitText(el, { type: "lines" });
			// Add some depth for the 3D rotation
			gsap.set(el, { perspective: 600 });
			const tl = gsap.timeline({
				scrollTrigger: {
					trigger: el,
					start: "top 85%",          // a bit earlier for smoother reveal
					end: "bottom 60%",
					scrub: false,
					markers: false,
					toggleActions: "play none none reverse", 
					// use "play none none none" if you don't want reverse
				}
			});
			tl.from(split.lines, {
				opacity: 0,
				y: 25,
				rotationX: -60,
				transformOrigin: "top center",
				duration: 1.1,
				ease: "power3.out",
				stagger: 0.08
			});
		});
	}

	// Ukiyo.js Parallax
	let parallax = null;
	if (typeof Ukiyo !== "undefined") {
		parallax = new Ukiyo(".ukiyo", {
			externalRAF: true,
		});
	}

	// Lenis smooth scroll - buttery, momentum-based scrolling across the site.
	// Guarded: if the library is missing or the visitor prefers reduced motion
	// we simply fall back to the browser's native scrolling (nothing else in
	// this file depends on `lenis` existing).
	let lenis = null;
	const prefersReducedMotion =
		typeof window.matchMedia === "function" &&
		window.matchMedia("(prefers-reduced-motion: reduce)").matches;

	if (typeof Lenis !== "undefined" && !prefersReducedMotion) {
		lenis = new Lenis({
			duration: 1.1,
			easing: (t) => Math.min(1, 1.001 - Math.pow(2, -10 * t)),
			smoothWheel: true,
			wheelMultiplier: 1,
			touchMultiplier: 1.6,
			smoothTouch: false,
		});
		// Keep ScrollTrigger's positions in sync with the virtual scroll.
		if (typeof ScrollTrigger !== "undefined") {
			lenis.on("scroll", () => ScrollTrigger.update());
		}
		// Exposed so other scripts (back-to-top, anchor links) can share the
		// same easing instead of fighting it with window.scrollTo.
		window.fhpLenis = lenis;
	}

	// Master RAF loop
	function raf(time) {
		// Only animate Ukiyo if it exists
		if (parallax) {
			parallax.animate();
		}
		if (lenis) lenis.raf(time);
		requestAnimationFrame(raf);
	}
	requestAnimationFrame(raf);
	
	// Init everything after DOM is ready
	document.addEventListener("DOMContentLoaded", () => {
		initSplitTextAnimation();
	});
    
})();