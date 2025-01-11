import HtmlWebpackPlugin from "html-webpack-plugin";
import { resolve } from "path";

/** @type {import("webpack").Configuration} */
export default {
  target: "web",
  entry: "./src/main.ts",
  output: { path: resolve("./wwwroot") },
  resolve: {
    extensions: [".ts", ".js"],
    modules: ["./src", "./node_modules"],
  },
  module: {
    rules: [
      { test: /\.ts$/, loader: "ts-loader", exclude: /node_modules/ },
      { test: /\.html$/, loader: "html-loader", exclude: /node_modules/ },
      {
        test: /\.css$/,
        use: [{ loader: "style-loader" }, { loader: "css-loader" }],
      },
    ],
  },
  plugins: [new HtmlWebpackPlugin({ template: "index.html" })],
  experiments: {
    topLevelAwait: true,
  },
};
