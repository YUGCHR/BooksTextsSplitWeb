const SET_SENTENCES_COUNT = 'SET-SENTENCES-COUNT';
const SET_SENTENCES = 'SET-SENTENCES';
const TOGGLE_IS_FETCHING = 'TOGGLE-IS-FETCHING';

let initialState = {    
    allBookIdsWithNames: [],
    sentencesCount: [777, 888], //engSentencesCount: 777, rusSentencesCount: 888
    emptyVariable: null,
    isTextLoaded: [false, false],
    isFetching: false
}

const selectTextsReducer = (state = initialState, action) => {

    switch (action.type) {        
        case SET_SENTENCES_COUNT:
            {
                // let allBookIdsWithNames = state.allBookIdsWithNames;
                // action.bookIds.map((id, i) => {
                //     if( id > 0)
                //     {
                //         {allBookIdsWithNames[i] = id};
                //     }
                // })
                // console.log(allBookIdsWithNames);
                //debugger;
                // let actionBookIds = action.bookIds;
                // return { ...state, actionBookIds };



                let stateCopy = {...state };
                //stateCopy.engSentences = {...state.engSentences };
                stateCopy.allBookIdsWithNames = action.bookIds;

// console.log('state',state.allBookIdsWithNames);
// console.log('copy',stateCopy.allBookIdsWithNames);


                return stateCopy;
            }
            case SET_SENTENCES:
                {
                    let stateCopy = {...state };
                    stateCopy.engSentences = {...state.engSentences };
                    stateCopy.engSentences = action.sentences;
                    return stateCopy;
                }        
        case TOGGLE_IS_FETCHING:
            {
                return {...state, isFetching: action.isFetching };
            }
        default:
            return state;
    }
}


export const setAllBookIdsWithNames = (bookIds) => ({ type: SET_SENTENCES_COUNT, bookIds });
export const setSentences = (sentences, languageId) => ({ type: SET_SENTENCES, sentences, languageId });

export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default selectTextsReducer;