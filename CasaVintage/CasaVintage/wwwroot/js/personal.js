// Comportamientos de la vista "Personal de la empresa".
// Progresivo: sin JS las tarjetas se ven y las acciones funcionan igual (POST normales).

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        activarInfo();
        activarDialogoRestablecer();
        activarRevelado();
        activarVistaPreviaFoto();
    });

    // Previsualiza la imagen elegida antes de enviarla. Funciona para uno o varios campos: cada
    // input [data-foto-input] muestra su vista previa en el [data-foto-previa] de su mismo grupo.
    function activarVistaPreviaFoto() {
        document.querySelectorAll("[data-foto-input]").forEach(function (input) {
            var grupo = input.closest("[data-foto-grupo]") || input.parentElement;
            var previa = grupo ? grupo.querySelector("[data-foto-previa]") : null;
            if (!previa) {
                return;
            }

            input.addEventListener("change", function () {
                var archivo = input.files && input.files[0];
                if (!archivo) {
                    return;
                }
                var url = URL.createObjectURL(archivo);
                previa.innerHTML = "";
                var img = document.createElement("img");
                img.src = url;
                img.alt = "Vista previa";
                img.addEventListener("load", function () {
                    URL.revokeObjectURL(url);
                });
                previa.appendChild(img);
            });
        });
    }

    // Panel "Info": expande/colapsa los datos y acciones de cada empleado.
    function activarInfo() {
        document.querySelectorAll("[data-info-toggle]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                var panel = document.getElementById(boton.getAttribute("aria-controls"));
                if (!panel) {
                    return;
                }
                var abierto = boton.getAttribute("aria-expanded") === "true";
                boton.setAttribute("aria-expanded", String(!abierto));
                boton.classList.toggle("abierto", !abierto);
                panel.hidden = abierto;
            });
        });
    }

    // Dialogo unico para restablecer contrasena: un boton por tarjeta lo abre con el id y nombre.
    function activarDialogoRestablecer() {
        var dialogo = document.getElementById("dlgRestablecer");
        if (!dialogo) {
            return;
        }

        var campoId = dialogo.querySelector("input[name='Restablecer.IdUsuario']");
        var nombre = document.getElementById("dlgRestablecerNombre");
        var campoPass = dialogo.querySelector("input[name='Restablecer.Password']");

        function abrir() {
            if (typeof dialogo.showModal === "function") {
                dialogo.showModal();
            } else {
                dialogo.setAttribute("open", "");
            }
            if (campoPass) {
                campoPass.focus();
            }
        }

        document.querySelectorAll("[data-restablecer]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                if (campoId) {
                    campoId.value = boton.getAttribute("data-id") || "";
                }
                if (nombre) {
                    nombre.textContent = boton.getAttribute("data-nombre") || "Empleado";
                }
                abrir();
            });
        });

        dialogo.querySelectorAll("[data-cerrar-dialogo]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                dialogo.close();
            });
        });

        // Si el servidor pidio reabrir (fallo la validacion), se muestra con los errores.
        if (window.CasaVintagePersonal && window.CasaVintagePersonal.reabrirRestablecer) {
            abrir();
        }
    }

    // Revelado suave de las tarjetas al hacer scroll. El contenido ya es visible por defecto:
    // solo si hay JS y el usuario no pidio reducir el movimiento se aplica la animacion.
    function activarRevelado() {
        var lista = document.querySelector("[data-reveal]");
        if (!lista || !("IntersectionObserver" in window)) {
            return;
        }
        if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            return;
        }

        var filas = Array.prototype.slice.call(lista.querySelectorAll(".personal-fila"));
        if (!filas.length) {
            return;
        }

        lista.classList.add("con-revelado");
        filas.forEach(function (fila) {
            fila.classList.add("por-revelar");
        });

        function revelar(fila) {
            fila.classList.add("revelada");
        }

        var observador = new IntersectionObserver(function (entradas) {
            entradas.forEach(function (entrada) {
                if (entrada.isIntersecting) {
                    revelar(entrada.target);
                    observador.unobserve(entrada.target);
                }
            });
        }, { threshold: 0.15, rootMargin: "0px 0px -40px 0px" });

        filas.forEach(function (fila) {
            observador.observe(fila);
        });

        // Salvaguarda: si el observador no dispara (pestana en segundo plano o render sin
        // pintar), se quita el estado oculto para que el contenido NUNCA quede invisible.
        // Se retira la clase (no se anima) porque las transiciones no avanzan sin pintado.
        setTimeout(function () {
            filas.forEach(function (fila) {
                fila.classList.remove("por-revelar");
            });
        }, 700);
    }
})();
