import React from 'react';
import s from './Header.module.css';
import { NavLink } from 'react-router-dom';
import logoPicture from '../../assets/images/logoPicture.png';

const Header = (props) => {

    //let readingSentence = props.readAndTranslatePage.readingSentenceNumber;

    return (<header className={s.header}>
        <div>           
        <div className={s.loginBlock}>
          {props.isAuth ? (
            <div>
              {props.login} - <button onClick={props.logout}>Log out</button>
            </div>
          ) : (
            <NavLink to="/login">Login</NavLink>
          )}
        </div>
        </div>
        <div className={s.logoPicture}>
            <img src={logoPicture} alt="" />
        </div>

    </header>);
}

export default Header;
