import { createStore, combineReducers } from "redux";
import readAndTranslateReducer from "./read-reducer";
import uploadBooksReducer from "./load-reducer";

let reducers = combineReducers({
    readAndTranslatePage: readAndTranslateReducer,
    uploadBooksPage: uploadBooksReducer
    
})

let store = createStore(reducers);

window.store = store;

export default store;
