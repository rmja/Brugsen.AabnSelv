import "bootstrap/dist/css/bootstrap.css";
import "bootstrap/dist/js/bootstrap";

import { ICustomElementViewModel, customElement } from "aurelia";

import { AuthHandler } from "./oauth";
import { routes } from "@aurelia/router";
import template from "./app-root.html";

@routes([
  { path: "", redirectTo: "signup" },
  {
    id: "signup",
    path: "signup",
    component: import("./signup/router"),
    title: "Brugeroprettelse",
  },
  {
    id: "admin",
    path: "admin",
    component: import("./admin/router"),
    title: "Administration",
  },
  {
    path: "signin-oidc",
    component: AuthHandler,
  },
])
@customElement({
  name: "app-root",
  template,
})
export class AppRootCustomElement implements ICustomElementViewModel {
  constructor() {}
}
