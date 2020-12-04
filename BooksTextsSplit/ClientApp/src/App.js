import React, { Component, lazy, Suspense } from "react"; // eslint-disable-line no-unused-vars
import { connect } from "react-redux";
import { compose } from "redux";
import { BrowserRouter, Redirect, Route, Switch, withRouter } from "react-router-dom";
import { initializeApp } from "./redux/app-reducer";
import { Provider } from "react-redux";
import { withSuspense } from "./hoc/withSuspense";
import store from "./redux/redux-store";
import "./App.css";
import HeaderContainer from "./components/Header/HeaderContainer";
import FooterContainer from "./components/Footer/FooterContainer";
//import Header from "./components/Header/Header";
import Navbar from "./components/Navbar/Navbar";
import Preloader from "./components/common/preloader/Preloader";
import SelectTextsContainer from "./components/SelectTexts/SelectTextsContainer";
import WordsToPair from "./components/WordsToPair/WordsToPair";
import UserProfile from "./components/UserProfile/UserProfile";
import Settings from "./components/Settings/Settings";
// TODO менять страницу из таблицы на вертикаль при ширине меньше 800 пикселей
const LoginPage = lazy(() => import("./components/Login/Login"));
const UploadPage = lazy(() => import("./components/UploadBooks/UploadBooksContainer"));
const ReadPage = lazy(() => import("./components/ToReadAndTranslate/ToReadAndTranslateContainer"));
// TODO всплывающая подсказка на элементах
class App extends Component {
  catchAllUnhandledErrors = (reason, promise) => {
    //alert("Some error occured");
    console.error("catchAllUnhandledErrors", reason, promise);
  };
  componentDidMount() {
    // TODO - Get all user data from Redis and return it to UI
    this.props.initializeApp(); //thunk
    window.addEventListener("unhandledrejection", this.catchAllUnhandledErrors);
  }
  componentWillUnmount() {
    window.removeEventListener("unhandledrejection", this.catchAllUnhandledErrors);
  }
  render() {
    if (!this.props.initialized) {
      return <Preloader />; // TODO полупрозрачный, не сдвигающий разметку, масштабирующийся по размеру окна
    }
    // this.props.history.push("/login");
    return (
      <div className="app-wrapper">
        <HeaderContainer />
        <Navbar />

        <div className="app-wrapper-content">
          <Switch>
            <Route exact path="/" render={() => <Redirect to={"/select"} />} />
            <Route path="/upload" render={withSuspense(UploadPage)} />
            <Route path="/select" render={() => <SelectTextsContainer />} />
            <Route path="/read" render={withSuspense(ReadPage)} />
            <Route path="/words" render={WordsToPair} />
            <Route path="/user" render={UserProfile} />
            <Route path="/settings" render={Settings} />
            <Route path="/login" render={withSuspense(LoginPage)} />
            <Route path="*" render={() => <div>404 NOT FOUND</div>} />
          </Switch>
        </div>
        <FooterContainer />
      </div>
    );
  }
}

let mapStateToProps = (state) => ({
  initialized: state.app.initialized,
});

const AppContainer = compose(withRouter, connect(mapStateToProps, { initializeApp }))(App);

const MainApp = () => {
  return (
    <BrowserRouter>
      <Provider store={store}>
        <AppContainer />
      </Provider>
    </BrowserRouter>
  );
};

export default MainApp;
