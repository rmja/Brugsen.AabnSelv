let config: Config;
switch (import.meta.env.VITE_CONFIG || import.meta.env.MODE) {
  case "development":
    config = await import("./config.development.json");
    break;
  default:
    config = await import("./config.production.json");
    break;
}
export default config;
