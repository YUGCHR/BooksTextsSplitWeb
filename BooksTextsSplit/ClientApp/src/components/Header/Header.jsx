import React from "react";
import s from "./Header.module.css";
import cs from "../../Common.module.css";
import { NavLink } from "react-router-dom";
import logoPicture from "../../assets/images/1219135_open-book-logo.png";
import Preloader from "../common/preloader/Preloader";

const Header = (props) => {
  //let readingSentence = props.readAndTranslatePage.readingSentenceNumber;
  return (
    <header className={s.header}>
      <div className={s.headerPartsPlace}>
        <img className={s.logoPicture} src={logoPicture} alt="" />
        {/* <div>This will be header menu/info block</div> */}
        <div className={s.preloaderPlace}>{props.isFetching ? <Preloader /> : null}</div>
        <div className={s.preloaderPlace}>{props.whoCalled}</div>
        <div className={s.loginBlock}>
          {/* TODO to fetch users login from server cookies and show here */}
          {props.isAuth ? (
            <div>
              <div>
                {"You are logged in as "}
                <button className={cs.allButtonsBodies} onClick={props.logout}>
                  Log out
                </button>
              </div>
              <div>{props.email}</div> {/* TODO it's necessary to pass here login Name */}
            </div>
          ) : (
            <NavLink to="/login">Login</NavLink>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;
