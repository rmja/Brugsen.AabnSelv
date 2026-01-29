import { IRouteViewModel, route } from "@aurelia/router";

import { customElement } from "aurelia";
import template from "./signup-router.html";

@route({
  routes: [
    {
      id: "signup",
      path: "",
      component: import("./contact-step"),
      title: "Brugeroprettelse",
    },
    {
      id: "store-confirmation",
      path: "store-confirmation",
      component: import("./store-confirmation-step"),
      title: "Butiksbekræftelse",
    },
    {
      id: "receipt",
      path: "receipt",
      component: import("./receipt-step"),
      title: "Kvittering",
    },
  ],
})
@customElement({ name: "signup-router", template })
export class SignupRouter implements IRouteViewModel {}
