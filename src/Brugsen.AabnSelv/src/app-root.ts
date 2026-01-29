import "bootstrap/dist/css/bootstrap.css";
import "bootstrap/dist/js/bootstrap";

import { route } from "@aurelia/router";

await import("./signup/contact-step");
await import("./signup/store-confirmation-step");
await import("./signup/receipt-step");

@route({
  title: "Brugsen Vesterø Havn",
  routes: [
    { path: "", redirectTo: "signup" },
    {
      path: "signup",
      component: () => import("./signup/signup-router"),
    },
    {
      path: "admin",
      component: () => import("./admin/admin-router"),
      title: "Administration",
    },
    {
      path: "signin-oidc",
      component: () => import("./auth/auth-handler"),
    },
  ],
})
export class AppRoot {}
