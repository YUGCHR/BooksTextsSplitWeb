import { createStore, combineReducers, applyMiddleware, compose } from "redux";
import readAndTranslateReducer from "./read-reducer";
import uploadBooksReducer from "./load-reducer";
import selectTextsReducer from "./select-reducer";
import authReducer from "./auth-reducer";
import appReducer from "./app-reducer";
import thunkMiddleware from "redux-thunk";
import { reducer as formReducer } from "redux-form";

let reducers = combineReducers({
  readAndTranslatePage: readAndTranslateReducer,
  selectTextsPage: selectTextsReducer,
  uploadBooksPage: uploadBooksReducer,
  auth: authReducer,
  app: appReducer,
  form: formReducer,
});

const composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;
const store = createStore(reducers, composeEnhancers(applyMiddleware(thunkMiddleware)));

window.__store__ = store;

export default store;
