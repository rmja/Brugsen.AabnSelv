import "bootstrap/dist/css/bootstrap.css";
import "bootstrap/dist/js/bootstrap";

import { ICustomElementViewModel, customElement } from "aurelia";

import { AuthHandler } from "./oauth";
import { route } from "@aurelia/router";
import template from "./app-root.html";

@route({
  title: "Brugsen Vesterø Havn",
  routes: [
    { path: "", redirectTo: "signup" },
    {
      id: "signup",
      path: "signup",
      component: import("./signup/signup-router"),
    },
    {
      id: "admin",
      path: "admin",
      component: import("./admin/admin-router"),
      title: "Administration",
    },
    {
      path: "signin-oidc",
      component: AuthHandler,
    },
  ],
})
@customElement({
  name: "app-root",
  template,
})
export class AppRootCustomElement implements ICustomElementViewModel {}
