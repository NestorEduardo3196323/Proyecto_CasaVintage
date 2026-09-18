// Client behaviors for La Casa Vintage.

document.addEventListener("DOMContentLoaded", function () {
    // Welcome card: the "Enter" button only closes the overlay to reveal the panel.
    // The "X" is not handled here because it submits the sign-out form (do not enter).
    var overlay = document.getElementById("bienvenidaOverlay");
    var btnIngresar = document.getElementById("btnIngresarBienvenida");

    if (overlay && btnIngresar) {
        btnIngresar.addEventListener("click", function () {
            overlay.remove();
        });
    }

    // Global confirmation for sensitive actions: any button with [data-confirmar] asks for
    // confirmation before submitting its form (delete supplier, disable account, etc.).
    document.querySelectorAll("[data-confirmar]").forEach(function (boton) {
        boton.addEventListener("click", function (evento) {
            if (!window.confirm(boton.getAttribute("data-confirmar"))) {
                evento.preventDefault();
            }
        });
    });

    // Client-side table search: filters the rows by the typed text.
    document.querySelectorAll("[data-tabla-buscar]").forEach(function (input) {
        var tabla = document.querySelector(input.getAttribute("data-tabla-buscar"));
        if (!tabla) {
            return;
        }
        var vacio = input.getAttribute("data-sin-resultados")
            ? document.querySelector(input.getAttribute("data-sin-resultados"))
            : null;

        input.addEventListener("input", function () {
            var termino = input.value.trim().toLowerCase();
            var visibles = 0;
            tabla.querySelectorAll("tbody tr").forEach(function (fila) {
                var coincide = fila.textContent.toLowerCase().indexOf(termino) !== -1;
                fila.hidden = !coincide;
                if (coincide) {
                    visibles++;
                }
            });
            if (vacio) {
                vacio.hidden = visibles !== 0;
            }
        });
    });

    // Side menu on mobile: the button opens it and the dark backdrop (or the Esc key) closes it.
    var shell = document.getElementById("appShell");
    if (shell) {
        document.querySelectorAll("[data-abrir-menu]").forEach(function (abrir) {
            abrir.addEventListener("click", function () {
                shell.classList.add("menu-abierto");
            });
        });
        shell.querySelectorAll("[data-cerrar-menu]").forEach(function (el) {
            el.addEventListener("click", function () {
                shell.classList.remove("menu-abierto");
            });
        });
        document.addEventListener("keydown", function (evento) {
            if (evento.key === "Escape") {
                shell.classList.remove("menu-abierto");
            }
        });
    }
});

// Floating notice (toast). Global so other scripts (cart) can use it.
window.mostrarToast = function (mensaje, tipo) {
    var cont = document.getElementById("toastCont");
    if (!cont) {
        cont = document.createElement("div");
        cont.id = "toastCont";
        cont.className = "toast-cont";
        document.body.appendChild(cont);
    }
    var toast = document.createElement("div");
    toast.className = "toast-vintage " + (tipo === "error" ? "es-error" : "es-ok");
    toast.textContent = mensaje;
    cont.appendChild(toast);
    // Force the reflow so the entry transition runs.
    void toast.offsetWidth;
    toast.classList.add("visible");
    window.setTimeout(function () {
        toast.classList.remove("visible");
        window.setTimeout(function () { toast.remove(); }, 300);
    }, 3000);
};

// Updates the cart icon counter in the menu (called by the "Add" button).
window.actualizarCarritoBadge = function (total) {
    var badge = document.getElementById("carritoBadge");
    if (!badge) {
        return;
    }
    badge.textContent = total;
    badge.hidden = !total || total <= 0;
};
