import React from 'react';
import s from './Navbar.module.css';
import { NavLink } from 'react-router-dom';

const Navbar = () => {
  return (<nav className={s.nav}>

    <div className={`${s.item} ${s.activeLink}`}>
      <NavLink to='/upload' activeClassName={s.activeLink}>UploadBooks</NavLink>
    </div>

    <div className={`${s.item} ${s.activeLink}`}>
      <NavLink to='/select' activeClassName={s.activeLink}>SelectTexts</NavLink>
    </div>

    <div className={`${s.item} ${s.activeLink}`}>
      <NavLink to='/read' activeClassName={s.activeLink}>ReadABook</NavLink>
    </div>

    <div className={`${s.item} ${s.activeLink}`}>
      <NavLink to='/words' activeClassName={s.activeLink}>WordsToPair</NavLink>
    </div>

    <div className={s.blank}> - </div>

    <div className={`${s.item} ${s.activeLink}`}>
      <NavLink to='/user' activeClassName={s.activeLink}>UserProfile</NavLink>
    </div>

    <div className={s.blank}> - </div>

    <div className={`${s.item} ${s.activeLink}`}>
      <NavLink to='/settings' activeClassName={s.activeLink}>Settings</NavLink>
    </div>

  </nav>);
}

export default Navbar;
