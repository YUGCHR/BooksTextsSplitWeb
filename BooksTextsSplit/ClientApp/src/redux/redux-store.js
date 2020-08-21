import { createStore, combineReducers, applyMiddleware } from "redux";
import readAndTranslateReducer from "./read-reducer";
import uploadBooksReducer from "./load-reducer";
import selectTextsReducer from "./select-reducer";
import thunkMiddleware from "redux-thunk";

let reducers = combineReducers({
    readAndTranslatePage: readAndTranslateReducer,
    selectTextsPage: selectTextsReducer,
    uploadBooksPage: uploadBooksReducer
})

let store = createStore(reducers, applyMiddleware(thunkMiddleware));

window.store = store;

export default store;
