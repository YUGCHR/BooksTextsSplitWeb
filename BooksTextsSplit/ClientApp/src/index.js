import React from "react";
import ReactDOM from "react-dom";
import * as serviceWorker from "./serviceWorker";
import "./index.css";
import MainApp from "./App";

//let reRenderEntireTree = () => {
/* ReactDOM.render(
        <BrowserRouter>       
            <Provider store={store}>
                <App />
            </Provider>
        </BrowserRouter>, document.getElementById('root'));
 */
ReactDOM.render(<MainApp />, document.getElementById("root"));

//reRenderEntireTree();

//store.subscribe(reRenderEntireTree);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
