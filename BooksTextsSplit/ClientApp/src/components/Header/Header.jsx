import React from 'react';
import s from './Header.module.css';
import { NavLink } from 'react-router-dom';
import logoPicture from '../../assets/images/logoPicture.png';

const Header = (props) => {

    //let readingSentence = props.readAndTranslatePage.readingSentenceNumber;

    return (<header className={s.header}>
        <div>           
            
        </div>
        <div className={s.logoPicture}>
            <img src={logoPicture} />
        </div>

    </header>);
}

export default Header;