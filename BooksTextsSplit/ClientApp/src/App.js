import React from 'react';
import './App.css';
import Header from './components/Header/Header';
import Navbar from './components/Navbar/Navbar';
import UploadBooksContainer from './components/UploadBooks/UploadBooksContainer';
import SelectTextsContainer from './components/SelectTexts/SelectTextsContainer';
import ToReadAndTranslateContainer from './components/ToReadAndTranslate/ToReadAndTranslateContainer';
import WordsToPair from './components/WordsToPair/WordsToPair';
import UserProfile from './components/UserProfile/UserProfile';
import Settings from './components/Settings/Settings';
import { Route } from 'react-router-dom';

const App = () => {

  return (
    <div className='app-wrapper'>
      
      <Header />
      <Navbar />

      <div className='app-wrapper-content'>
        <Route path='/upload' render={() => <UploadBooksContainer />} />
        <Route path='/select' render={() => <SelectTextsContainer />} />
        <Route path='/read' render={() => <ToReadAndTranslateContainer />} />
        <Route path='/words' render={WordsToPair} />
        <Route path='/user' render={UserProfile} />
        <Route path='/settings' render={Settings} />
      </div>
    </div>
  );
}

export default App;
