import React from "react";
import s from "./Header.module.css";
import { NavLink } from "react-router-dom";
import logoPicture from "../../assets/images/logoPicture.png";

const Header = (props) => {
  //let readingSentence = props.readAndTranslatePage.readingSentenceNumber;

  return (
    <header className={s.header}>
      <div className={s.loginPlace}>
        <img className={s.logoPicture} src={logoPicture} alt="" />

        <div className={s.loginBlock}>
          {props.isAuth ? (
            <div>
              {props.login}
              <button onClick={props.logout}>Log out</button>
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
