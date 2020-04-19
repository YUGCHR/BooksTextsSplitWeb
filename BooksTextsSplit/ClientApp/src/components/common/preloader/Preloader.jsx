import React from 'react';
import loadingImg from '../../../assets/images/tenor.gif';

let Preloader = (props) => {
    return (
        <div>
            <img src={loadingImg} />
        </div>
    )
}

export default Preloader;
