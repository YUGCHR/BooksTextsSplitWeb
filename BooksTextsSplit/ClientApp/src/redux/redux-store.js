import { createStore, combineReducers } from "redux";
import readAndTranslateReducer from "./read-reducer";
import uploadBooksReducer from "./load-reducer";
import selectTextsReducer from "./select-reducer";

let reducers = combineReducers({
    readAndTranslatePage: readAndTranslateReducer,
    selectTextsPage: selectTextsReducer,
    uploadBooksPage: uploadBooksReducer
})

let store = createStore(reducers);

window.store = store;

export default store;
