import { uploadAPI } from "../api/api";
import { toggleIsFetching } from "./app-reducer";

const SET_DB_SENTENCES_COUNT = "SET-DB-SENTENCES-COUNT";
const SET_SENTENCES_COUNT = "SET-SENTENCES-COUNT";
const SET_TASK_DONE_PERCENTS = "SET-TASK-DONE-PERCENTS";
const SET_FILE_NAME = "SET-FILE-NAME";
const SET_BOOKS_DESCRIPTIONS = "SET-BOOKS-DESCRIPTIONS";
const TOGGLE_IS_FETCHING = "TOGGLE-IS-FETCHING";
const TOGGLE_IS_LOADING = "TOGGLE-IS-LOADING";
const TOGGLE_IS_DONE_UPLOAD = "TOGGLE-IS-DONE-UPLOAD";
const TOGGLE_IS_WRONG_COUNT = "TOGGLE-IS-WRONG-COUNT";
const TOGGLE_UPLOAD_BUTTON_ENABLE = "TOGGLE-UPLOAD-BUTTON-ENABLE";
const RADIO_DEFAULT = "RADIO-DEFAULT";
const RADIO_IS_CHANGED = "RADIO-IS-CHANGED";
const SHOW_HIDE_STATE = "SHOW-HIDE-STATE";
const FIND_MAX_UPLOADED = "FIND-MAX-UPLOADED";

/* TODO
uploadPage - 
первый запрос - за названиями всего
загрузка файлов почти в любом количестве без особого контроля - 
проверять, что разрешённого сегодня (в этой версии) формата и не слишком дофига
после загрузки записать тексты в редис
анализировать - проверить есть ли шапки, какой язык текста, ещё что-то
можно окончательный анализ, если автоматически определился язык текста
отправить пользователю список загруженных (вообще доступных - загруженных, но не залитых в базу) 
файлов для разбора по парам, проверки языка и названий
для записи в редис генерировать токен, который потом пойдёт в таск для загрузки в базу */

let initialState = {
  selectedFiles: null, // used in ShowSelectedFiles
  radioChosenLanguage: ["eng", "rus"], // here default values of radio buttons to choose language
  radioAutoChangeLang: [
    ["eng", "rus"],
    ["rus", "eng"],
  ],
  radioAutoChangeLangInversed: [
    ["rus", "eng"],
    ["eng", "rus"],
  ],
  filesDescriptions: {
    index: "File No: ",
    name: "File name: ",
    lastMod: "Last modified: ",
    size: "File size: ",
    type: "File type: ",
    chosenLanguage: "Chosen file language: ",
  }, // used in ShowSelectedFiles
  uploadBooksLabels: {
    uploadBooksHeader1: "UPLOAD BOOKS ",
    uploadBooksHeader2: "CONTROL PANEL ",
    dbInfoHeader: "DB info",
    nearShowButton: " records, details - ",
    uploadButton: "Upload",
  },
  filesLanguageIds: [
    {
      languageId: 0,
      languageShortName: "eng",
      languageName: "English",
    },
    {
      languageId: 1,
      languageShortName: "rus",
      languageName: "Russian",
    },
  ],
  booksTitles: [{}, {}],
  sentencesCount: [-1, -2, -3, -4, -5],
  dbSentencesCount: [-7, -8],
  emptyVariable: null,
  isTextLoaded: [false, false],
  isFetching: false,
  uploadedVersions: [],
  maxUploadedVersion: -1,
  labelShowHide: [
    { label: "Show", value: false }, // value - are details shown
    { label: "Hide", value: true },
  ],
  isDoneUpload: false,
  isUploadButtonDisabled: true,
  isWrongCount: false,
  metadataHeader: "6L1n2qR1yzE0IjTZpUksGkbzF23vVGZeR0nEXL6qKhdXBGoJzSKqE9a1g",
  taskDonePercents: [0, 0],
  endWhilePercents: [99],
  whoCalledPreloader: "",
};

const uploadBooksReducer = (state = initialState, action) => {
  switch (action.type) {
    case TOGGLE_IS_LOADING: {
      /* return { ...state, isEngLoaded: action.isEngLoaded } */
      let stateCopy = { ...state };
      stateCopy.isTextLoaded = { ...state.isTextLoaded };
      stateCopy.isTextLoaded[action.languageId] = action.isTextLoaded;
      return stateCopy;
    }
    case SET_DB_SENTENCES_COUNT: {
      let stateCopy = { ...state };
      stateCopy.dbSentencesCount = { ...state.dbSentencesCount };
      stateCopy.dbSentencesCount[action.languageId] = action.count;
      return stateCopy;
    }
    case SET_SENTENCES_COUNT: {
      let stateCopy = { ...state };
      stateCopy.sentencesCount = { ...state.sentencesCount };
      stateCopy.sentencesCount[action.index] = action.count;
      return stateCopy;
    }
    case SET_TASK_DONE_PERCENTS: {
      let stateCopy = { ...state };
      stateCopy.taskDonePercents = { ...state.taskDonePercents };
      stateCopy.taskDonePercents[0] = action.response[0].doneInPercents;
      stateCopy.taskDonePercents[1] = action.response[1].doneInPercents;
      return stateCopy;
    }
    case SET_FILE_NAME: {
      return { ...state, selectedFiles: action.files };
    }
    case SET_BOOKS_DESCRIPTIONS: {
      return { ...state, booksTitles: action.textsMetadata };
    }
    case RADIO_IS_CHANGED: {
      let stateCopy = { ...state };
      stateCopy.radioChosenLanguage = { ...state.radioChosenLanguage };
      stateCopy.booksTitles = { ...state.booksTitles };
      let viceVersaBooksTitles = false;
      if (action.chosenLang === "eng") {
        stateCopy.radioChosenLanguage = state.radioAutoChangeLang[action.i];
        // если неправильный languageId - получено "eng", а он = 1, меняем местами booksTitles.languageId
        if (stateCopy.booksTitles[action.i].languageId === 1) {
          viceVersaBooksTitles = true;
        }
      } else {
        stateCopy.radioChosenLanguage = state.radioAutoChangeLangInversed[action.i];
        if (stateCopy.booksTitles[action.i].languageId === 0) {
          viceVersaBooksTitles = true;
        }
      }
      if (viceVersaBooksTitles) {
        [stateCopy.booksTitles[0].languageId, stateCopy.booksTitles[1].languageId] = [
          stateCopy.booksTitles[1].languageId,
          stateCopy.booksTitles[0].languageId,
        ];
      }
      return stateCopy;
    }
    case RADIO_DEFAULT: {
      let stateCopy = { ...state };
      stateCopy.radioChosenLanguage = { ...state.radioChosenLanguage };
      state.filesLanguageIds.map((fli) => {
        if (action.defaultLanguageId === fli.languageId) {
          stateCopy.radioChosenLanguage[action.i] = fli.languageShortName;
        }
      });
      return stateCopy;
    }
    case SHOW_HIDE_STATE: {
      let stateCopy = { ...state };
      stateCopy.labelShowHide = { ...state.labelShowHide };
      let tempValue = state.labelShowHide[0];
      stateCopy.labelShowHide[0] = state.labelShowHide[1];
      stateCopy.labelShowHide[1] = tempValue;
      return stateCopy;
    }
    case TOGGLE_IS_FETCHING: {
      if(action.isFetching){
      return { ...state, isFetching: action.isFetching, whoCalledPreloader: action.whoCalled };
      }
      else{
        return { ...state, isFetching: action.isFetching, whoCalledPreloader: "" };
      }
    }
    case TOGGLE_IS_DONE_UPLOAD: {
      return { ...state, isDoneUpload: action.isDoneUpload };
    }
    case TOGGLE_UPLOAD_BUTTON_ENABLE: {
      return { ...state, isUploadButtonDisabled: action.isUploadButtonDisabled };
    }
    case TOGGLE_IS_WRONG_COUNT: {
      return { ...state, isWrongCount: action.isWrongCount };
    }
    case FIND_MAX_UPLOADED: {
      let findMax = -1;
      action.uploadedVersions.map((u) => {
        if (u > findMax) {
          findMax = u;
        }
      });
      return { ...state, maxUploadedVersion: findMax };
    }
    default:
      return state;
  }
};

/* {
  ...state,
  ...action.payload,
  //isAuth: action.isAuth,
};
export const setAuthUserData = (userId, email, login, isAuth) => ({ type: SET_USER_DATA, payload: { userId, email, login, isAuth } });
 */

const setSentencesCount = (count, index) => ({ type: SET_SENTENCES_COUNT, count, index });
const setDbSentencesCount = (count, languageId) => ({ type: SET_DB_SENTENCES_COUNT, count, languageId });
const setTaskDonePercents = (response) => ({ type: SET_TASK_DONE_PERCENTS, response });

const toggleIsLoading = (isTextLoaded, languageId) => ({ type: TOGGLE_IS_LOADING, isTextLoaded, languageId });
//const toggleIsFetching = (isFetching, whoCalled) => ({ type: TOGGLE_IS_FETCHING, isFetching, whoCalled });
const toggleIsDoneUpload = (isDoneUpload) => ({ type: TOGGLE_IS_DONE_UPLOAD, isDoneUpload });
const toggleUploadButtonDisable = (isUploadButtonDisabled) => ({ type: TOGGLE_UPLOAD_BUTTON_ENABLE, isUploadButtonDisabled });

const setFileName = (files) => ({ type: SET_FILE_NAME, files });
const setBooksDescriptions = (textsMetadata) => ({ type: SET_BOOKS_DESCRIPTIONS, textsMetadata });

const wrongFilesCountSelected = (isWrongCount) => ({ type: TOGGLE_IS_WRONG_COUNT, isWrongCount });
const setRadioDefault = (defaultLanguageId, i) => ({ type: RADIO_DEFAULT, defaultLanguageId, i });
export const setRadioResult = (chosenLang, i) => ({ type: RADIO_IS_CHANGED, chosenLang, i }); // used in ShowSelectedFiles
export const setShowHideState = (chosenLang, i) => ({ type: SHOW_HIDE_STATE, chosenLang, i }); // used in ShowSelectedFiles

const fetchLastUploadedVersions = (bookTitle) => async (dispatch, getState) => {
  dispatch(toggleIsFetching(true, "fetchLastUploadedVersions"));
  const response = await uploadAPI.getLastUploadedVersions(bookTitle.bookId, bookTitle.languageId); // to find all previously uploaded versions of the file with this bookId
  dispatch(toggleIsFetching(false));
  bookTitle.uploadVersion = response.maxUploadedVersion;
  return bookTitle;
};

export const setFilesNamesAndEnableUpload = (files) => async (dispatch) => {
  //check user selected books pair
  dispatch(wrongFilesCountSelected(false));
  dispatch(toggleIsDoneUpload(false));
  if (files) {
    if (files.length === 2) {
      dispatch(setFileName(files));
      dispatch(toggleUploadButtonDisable(false));
      dispatch(setFilesMetadata(files));
    } else {
      dispatch(wrongFilesCountSelected(true));
    }
  } else {
    dispatch(setFileName(files));
    dispatch(toggleUploadButtonDisable(true));
  }
};

const setFilesMetadata = (files) => async (dispatch, getState) => {
  let textsMetadata = [{}, {}];
  for (let i = 0; i < files.length; i++) {
    //let file = files[i];
    let reader = new FileReader();
    reader.readAsText(files[i]);
    reader.onload = () => {
      // let textStrings = reader.result;
      const textFirst18Lines = reader.result.split("\n").slice(0, 18);
      //console.log(textFirst18Lines);
      if (textFirst18Lines[0].indexOf(getState().uploadBooksPage.metadataHeader) !== -1) {
        textsMetadata[i].bookId = parseInt(textFirst18Lines[2], 10);
        let currentLangId = parseInt(textFirst18Lines[4], 10);
        textsMetadata[i].languageId = currentLangId;
        dispatch(setRadioDefault(currentLangId, i));
        textsMetadata[i].authorNameId = parseInt(textFirst18Lines[6], 10);
        textsMetadata[i].authorName = textFirst18Lines[8];
        textsMetadata[i].bookNameId = parseInt(textFirst18Lines[10], 10);
        textsMetadata[i].bookName = textFirst18Lines[12];
        textsMetadata[i].comment = textFirst18Lines[14];
      }
    };
    reader.onerror = () => {
      console.log(reader.error);
    };
  }
  dispatch(setBooksDescriptions(textsMetadata));
};

const postBooksTexts = (formData, i) => async (dispatch) => {
  dispatch(toggleIsFetching(true, "postBooksTexts"));
  const response = await uploadAPI.uploadFile(formData); //post returns response before all records have loaded in db
  dispatch(toggleIsFetching(false));
  dispatch(setSentencesCount(response, i)); //totalCount
  return response;
};

const fetchTaskDonePercents = (taskGuid) => async (dispatch, getState) => {
  let response = [{}, {}];
  dispatch(toggleIsFetching(true, "fetchTaskDonePercents"));
  let percents = 0;
  while (percents < getState().uploadBooksPage.endWhilePercents) {
    response[0] = await uploadAPI.getUploadTaskPercents(taskGuid[0]);
    response[1] = await uploadAPI.getUploadTaskPercents(taskGuid[1]);
    percents = response[1].doneInPercents;
    dispatch(setTaskDonePercents(response));
  }
  response[0].doneInPercents = 100;
  response[1].doneInPercents = 100;
  dispatch(setTaskDonePercents(response));
  dispatch(toggleIsFetching(false));
};

export const fetchSentencesCount = (languageId) => async (dispatch, getState) => {
  dispatch(toggleIsFetching(true, "fetchSentencesCount"));
  const response = await uploadAPI.getSentenceCount(languageId);
  dispatch(toggleIsFetching(false));
  dispatch(setDbSentencesCount(response.sentencesCount, languageId));
  getState().uploadBooksPage.dbSentencesCount[languageId] === 0
    ? dispatch(toggleIsLoading(false, languageId))
    : dispatch(toggleIsLoading(true, languageId));
  return response.sentencesCount;
};

export const fileUploadHandler = (selectedFiles) => async (dispatch, getState) => {
  let response = [{}, {}];
  dispatch(toggleUploadButtonDisable(true));
  dispatch(toggleIsDoneUpload(true));
  for (let i = 0; i < selectedFiles.length; i++) {
    const form = new FormData();
    form.append("bookFile", selectedFiles[i], selectedFiles[i].name);
    // TODO it is possible to pass data in array (Object!) instead file properties
    const bookTitle = getState().uploadBooksPage.booksTitles[i]; //[0];
    dispatch(toggleIsFetching(true, "fileUploadHandler"));
    // to add maxUploadedVersion to formData it is necessary to find it in Cosmos Db
    const bookTitleWithVersion = await dispatch(fetchLastUploadedVersions(bookTitle));
    const bookTitleWithVersionJson = JSON.stringify(bookTitleWithVersion);
    form.append("jsonBookDescription", bookTitleWithVersionJson);
    response[i] = await dispatch(postBooksTexts(form, i));
    dispatch(toggleIsFetching(false));
  }
  await dispatch(fetchTaskDonePercents(response));
};

export default uploadBooksReducer;

// TODO
// возвращать весь класс textSentence
// YES - записывать проценты в массив
// YES - убрать запрос в базу после цикла
// переделать запрос в базу на редис
