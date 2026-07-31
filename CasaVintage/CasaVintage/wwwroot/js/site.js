// Comportamientos del cliente para La Casa Vintage.

document.addEventListener("DOMContentLoaded", function () {
    // Tarjeta de bienvenida: el boton "Ingresar" solo cierra el overlay para revelar el panel.
    // La "X" no se maneja aqui porque envia el formulario de cierre de sesion (no entrar).
    var overlay = document.getElementById("bienvenidaOverlay");
    var btnIngresar = document.getElementById("btnIngresarBienvenida");

    if (overlay && btnIngresar) {
        btnIngresar.addEventListener("click", function () {
            overlay.remove();
        });
    }

    // Confirmacion global para acciones sensibles: cualquier boton con [data-confirmar] pide
    // confirmacion antes de enviar su formulario (eliminar proveedor, desactivar cuenta, etc.).
    document.querySelectorAll("[data-confirmar]").forEach(function (boton) {
        boton.addEventListener("click", function (evento) {
            if (!window.confirm(boton.getAttribute("data-confirmar"))) {
                evento.preventDefault();
            }
        });
    });

    // Busqueda en tablas del lado del cliente: filtra las filas segun el texto escrito.
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

    // Menu lateral en movil: el boton lo abre y el fondo oscuro (o la tecla Esc) lo cierra.
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

// Aviso flotante (toast). Global para que otros scripts (carrito) lo usen.
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
    // Forzar el reflow para que la transicion de entrada corra.
    void toast.offsetWidth;
    toast.classList.add("visible");
    window.setTimeout(function () {
        toast.classList.remove("visible");
        window.setTimeout(function () { toast.remove(); }, 300);
    }, 3000);
};

// Actualiza el contador del icono del carrito en el menu (lo llama el boton "Agregar").
window.actualizarCarritoBadge = function (total) {
    var badge = document.getElementById("carritoBadge");
    if (!badge) {
        return;
    }
    badge.textContent = total;
    badge.hidden = !total || total <= 0;
};
