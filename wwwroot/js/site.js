// Fan Hub Plus - global JavaScript
// Theme (dark mode) + font size + page spinner + chat widget + AJAX rating/bookmark.
// Preferences are persisted in the cookie "fhp_pref" = "<dark 0|1>|<fontSize>"
// so the server can render the correct theme on the very next request (no flash).

(function () {
    'use strict';

    // ---------- Preference helpers ----------
    function readPref() {
        var m = document.cookie.match(/(?:^|;\s*)fhp_pref=([^;]+)/);
        if (m) {
            var parts = decodeURIComponent(m[1]).split('|');
            return { dark: parts[0] === '1', font: parseInt(parts[1] || '16', 10) };
        }
        var ls = localStorage.getItem('fhp_pref');
        if (ls) {
            var p = ls.split('|');
            return { dark: p[0] === '1', font: parseInt(p[1] || '16', 10) };
        }
        return { dark: false, font: 16 };
    }

    function writePref(pref) {
        var value = (pref.dark ? '1' : '0') + '|' + pref.font;
        document.cookie = 'fhp_pref=' + value + ';path=/;max-age=31536000;SameSite=Lax';
        localStorage.setItem('fhp_pref', value);
    }

    function applyPref(pref) {
        document.documentElement.classList.toggle('dark-mode', pref.dark);
        document.documentElement.style.fontSize = pref.font + 'px';
    }

    // Exposed to inline onclick handlers in the navbar
    window.fhpToggleTheme = function () {
        var pref = readPref();
        pref.dark = !pref.dark;
        writePref(pref); applyPref(pref);
        var btn = document.getElementById('themeToggle');
        if (btn) btn.textContent = pref.dark ? '☀️' : '🌙';
    };
    window.fhpFontStep = function (delta) {
        var pref = readPref();
        pref.font = Math.min(22, Math.max(14, pref.font + delta));
        writePref(pref); applyPref(pref);
    };

    // Guests: apply stored localStorage preference before anything renders
    applyPref(readPref());

    // ---------- Page spinner ----------
    function hideLoader() {
        var loader = document.getElementById('pageLoader');
        if (loader) loader.classList.add('hidden');
    }
    document.addEventListener('DOMContentLoaded', hideLoader);
    window.addEventListener('load', hideLoader);

    // ---------- Shared AJAX helper (attaches the antiforgery token) ----------
    function token() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    // ---------- Chat widget ----------
    document.addEventListener('DOMContentLoaded', function () {
        var widget = document.getElementById('chatWidget');
        var toggle = document.getElementById('chatToggle');
        if (!widget || !toggle) return;

        function open() {
            widget.classList.remove('d-none');
            toggle.style.display = 'none';
            var body = document.getElementById('chatBody');
            if (body) body.scrollTop = body.scrollHeight;
        }
        function close() { widget.classList.add('d-none'); toggle.style.display = ''; }

        toggle.addEventListener('click', open);
        var closeBtn = document.getElementById('chatClose');
        if (closeBtn) closeBtn.addEventListener('click', close);

        var body = document.getElementById('chatBody');
        var suggestions = document.getElementById('chatSuggestions');

        function addMsg(text, who) {
            var div = document.createElement('div');
            div.className = 'chat-msg ' + who;
            div.textContent = text;
            body.appendChild(div);
            body.scrollTop = body.scrollHeight;
            return div;
        }

        function renderSuggestions(list) {
            suggestions.innerHTML = '';
            (list || []).forEach(function (s) {
                var b = document.createElement('button');
                b.type = 'button'; b.className = 'chat-suggestion'; b.textContent = s;
                b.addEventListener('click', function () { send(s); });
                suggestions.appendChild(b);
            });
        }

        function send(text) {
            if (!text) return;
            addMsg(text, 'user');
            var typing = addMsg('typing…', 'bot typing');
            var fd = new FormData();
            fd.append('message', text);
            fd.append('__RequestVerificationToken', token());

            fetch('/Chatbot/Ask', { method: 'POST', body: fd, credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    typing.remove();
                    addMsg(data.reply || '(empty reply)', 'bot');
                    renderSuggestions(data.suggestions);
                })
                .catch(function () { typing.remove(); addMsg('Sorry, I could not reach the server.', 'bot'); });
        }

        document.getElementById('chatForm').addEventListener('submit', function (e) {
            e.preventDefault();
            var input = document.getElementById('chatText');
            send(input.value.trim());
            input.value = '';
        });
    });

    // ---------- Star rating (AJAX) ----------
    document.addEventListener('DOMContentLoaded', function () {
        var wrap = document.getElementById('ratingWidget');
        if (!wrap) return;
        var contentId = wrap.getAttribute('data-content-id');
        var stars = Array.prototype.slice.call(wrap.querySelectorAll('.star'));

        function paint(n, hover) {
            stars.forEach(function (s, i) {
                if (hover) s.classList.toggle('hover', i < n);
                else s.classList.toggle('on', i < n);
            });
        }

        stars.forEach(function (star, i) {
            star.addEventListener('mouseenter', function () { paint(i + 1, true); });
            star.addEventListener('mouseleave', function () {
                stars.forEach(function (s) { s.classList.remove('hover'); });
                paint(parseInt(wrap.getAttribute('data-my-stars') || '0', 10), false);
            });
            star.addEventListener('click', function () {
                var fd = new FormData();
                fd.append('contentId', contentId);
                fd.append('stars', String(i + 1));
                fd.append('__RequestVerificationToken', token());
                fetch('/Explore/Rate', { method: 'POST', body: fd, credentials: 'same-origin' })
                    .then(function (r) { return r.json(); })
                    .then(function (res) {
                        if (res.auth === false) {
                            window.location = '/Account/Login?returnUrl=' + encodeURIComponent(location.pathname);
                            return;
                        }
                        if (res.ok) {
                            wrap.setAttribute('data-my-stars', res.myStars);
                            paint(res.myStars, false);
                            var avg = document.getElementById('ratingAvg');
                            var cnt = document.getElementById('ratingCount');
                            if (avg) avg.textContent = res.avg;
                            if (cnt) cnt.textContent = '(' + res.count + ' ratings)';
                        } else if (res.message) { alert(res.message); }
                    });
            });
        });
        paint(parseInt(wrap.getAttribute('data-my-stars') || '0', 10), false);
    });

    // ---------- Bookmark toggle (AJAX) ----------
    document.addEventListener('DOMContentLoaded', function () {
        Array.prototype.forEach.call(document.querySelectorAll('.bookmark-btn'), function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                var fd = new FormData();
                fd.append('type', btn.getAttribute('data-type'));
                fd.append('itemId', btn.getAttribute('data-id'));
                fd.append('__RequestVerificationToken', token());
                fetch('/Bookmarks/Toggle', { method: 'POST', body: fd, credentials: 'same-origin' })
                    .then(function (r) { return r.json(); })
                    .then(function (res) {
                        if (res.auth === false) {
                            window.location = '/Account/Login?returnUrl=' + encodeURIComponent(location.pathname);
                            return;
                        }
                        if (res.ok) {
                            btn.classList.toggle('active', res.bookmarked);
                            btn.textContent = res.bookmarked ? '♥ Bookmarked' : '♡ Bookmark';
                            btn.title = res.message;
                        }
                    });
            });
        });
    });
})();

