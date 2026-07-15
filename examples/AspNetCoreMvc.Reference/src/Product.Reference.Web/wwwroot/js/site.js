window.setupWizard = {
    initialize: function (options) {
        const form = document.getElementById("setup-form");
        if (!form) return;

        const result = document.getElementById("test-result");
        const testButton = document.getElementById("test-connection");
        const auth = document.getElementById("authentication");

        function databaseType() {
            return form.querySelector('input[name="DatabaseType"]:checked')?.value || "Local";
        }

        function updateVisibility() {
            const server = databaseType() === "InternalServer";
            const sqlAuth = server && auth.value === "SqlServer";

            document.querySelectorAll(".server-only").forEach(function (element) {
                element.hidden = !server;
            });

            document.querySelectorAll(".sql-auth-only").forEach(function (element) {
                element.hidden = !sqlAuth;
            });

            result.textContent = "";
            result.className = "status";
        }

        form.querySelectorAll('input[name="DatabaseType"]').forEach(function (element) {
            element.addEventListener("change", updateVisibility);
        });

        auth.addEventListener("change", updateVisibility);

        testButton.addEventListener("click", async function () {
            result.textContent = "Testing connection...";
            result.className = "status";

            const response = await fetch(options.testUrl, {
                method: "POST",
                body: new FormData(form),
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const data = await response.json();

            result.textContent = data.message;
            result.className = data.success
                ? "status success"
                : "status error";
        });

        updateVisibility();
    }
};
