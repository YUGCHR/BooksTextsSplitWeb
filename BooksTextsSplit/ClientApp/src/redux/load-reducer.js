const SET_SENTENCES_COUNT = 'SET-SENTENCES-COUNT';
const SET_BOOK_TITLE = 'SET-BOOK-TITLE';
const TOGGLE_IS_FETCHING = 'TOGGLE-IS-FETCHING';
const TOGGLE_IS_LOADING = 'TOGGLE-IS-LOADING';

let initialState = {
    engTextTitle: [
        { languageId: 0, authorName: '1 Vernor Vinge', bookTitle: '1 A Fire Upon the Deep' },
        { languageId: 0, authorName: '2 Vernor Vinge', bookTitle: '2 A Fire Upon the Deep' },
        { languageId: 0, authorName: '3 Vernor Vinge', bookTitle: '3 A Fire Upon the Deep' },
        { languageId: 0, authorName: '4 Vernor Vinge', bookTitle: '4 A Fire Upon the Deep' },
        { languageId: 0, authorName: '5 Vernor Vinge', bookTitle: '5 A Fire Upon the Deep' },
    ],
    engSentences: [
        { languageId: 0, sentenceText: '01 How to explain?' },
        { languageId: 0, sentenceText: '02 How to describe?' },
        { languageId: 0, sentenceText: '03 Even the omniscient viewpoint quails.' },
        { languageId: 0, sentenceText: '04 * ' },
        { languageId: 0, sentenceText: '05 A singleton star, reddish and dim.' },
        { languageId: 0, sentenceText: '06 A ragtag of asteroids, and a single planet, more like a moon.' },
        { languageId: 0, sentenceText: '07 In this era the star hung near the galactic plane, just beyond the Beyond.' },
        { languageId: 0, sentenceText: '08 The structures on the surface were gone from normal view, pulverized into regolith across a span of aeons.' },
        { languageId: 0, sentenceText: '09 The treasure was far underground, beneath a network of passages, in a single room filled with black.' },
        { languageId: 0, sentenceText: '10 Information at the quantum density, undamaged.' },
        { languageId: 0, sentenceText: '11 Maybe five billion years had passed since the archive was lost to the nets.' },
        { languageId: 0, sentenceText: '12The curse of the mummy’s tomb, a comic image from mankind’s own prehistory, lost before time.' },
        { languageId: 0, sentenceText: '13 * ' },
        { languageId: 0, sentenceText: '14 They had laughed when they said it, laughed with joy at the treasure… and determined to be cautious just the same.' },
        { languageId: 0, sentenceText: '15 They would live here a year or five, the little company from Straum, the archaeologist programmers, their families and schools.' },
        { languageId: 0, sentenceText: '16 A year or five would be enough to handmake the protocols, to skim the top and identify the treasure’s origin in time and space, to learn a secret or two that would make Straumli Realm rich.' },
        { languageId: 0, sentenceText: '17 And when they were done, they would sell the location; perhaps build a network link (but chancier that  —  this was beyond the Beyond; who knew what Power might grab what they’d found).' },
        { languageId: 0, sentenceText: '18  * ' },
        { languageId: 0, sentenceText: '19 So now there was a tiny settlement on the surface, and they called it the High Lab.' },
        { languageId: 0, sentenceText: '20 It was really just humans playing with an old library.' },
        { languageId: 0, sentenceText: '21 It should be safe, using their own automation, clean and benign.' },
        { languageId: 0, sentenceText: '22 This library wasn’t a living creature, or even possessed of automation (which here might mean something more, far more, than human).' },
        { languageId: 0, sentenceText: '23 They would look and pick and choose, and be careful not to be burned…' },
        { languageId: 0, sentenceText: '24  * ' },
        { languageId: 0, sentenceText: '25 Humans starting fires and playing with the flames.' },
        { languageId: 0, sentenceText: '26 * ' },
        { languageId: 0, sentenceText: '27 The archive informed the automation.' },
        { languageId: 0, sentenceText: '28 Data structures were built, recipes followed.' },
        { languageId: 0, sentenceText: '29 A local network was built, faster than anything on Straum, but surely safe.' },
        { languageId: 0, sentenceText: '30 Nodes were added, modified by other recipes.' },
        { languageId: 0, sentenceText: '31 The archive was a friendly place, with hierarchies of translation keys that led them along.' },
        { languageId: 0, sentenceText: '32 Straum itself would be famous for this.' }
    ],
    lastSentenceNumber: null,
    rusTextTitle: [
        { languageId: 1, authorName: '1 Вернор Виндж', bookTitle: '1 Пламя над бездной' },
        { languageId: 1, authorName: '2 Вернор Виндж', bookTitle: '2 Пламя над бездной' },
        { languageId: 1, authorName: '3 Вернор Виндж', bookTitle: '3 Пламя над бездной' },
        { languageId: 1, authorName: '4 Вернор Виндж', bookTitle: '4 Пламя над бездной' },
        { languageId: 1, authorName: '5 Вернор Виндж', bookTitle: '5 Пламя над бездной' }
    ],
    rusSentences: [
        { languageId: 1, sentenceText: '01 Как объяснить?' },
        { languageId: 1, sentenceText: '02 Как описать?' },
        { languageId: 1, sentenceText: '03 Даже всезнание отказывает.' },
        { languageId: 1, sentenceText: '04  * ' },
        { languageId: 1, sentenceText: '05 Одиночная звезда, красноватая и тусклая.' },
        { languageId: 1, sentenceText: '06 Россыпь астероидов и единственная планета, больше похожая на луну.' },
        { languageId: 1, sentenceText: '07 В эту эпоху звезда повисла возле плоскости галактики, у самого Края.' },
        { languageId: 1, sentenceText: '08 Структуры поверхности давно потеряли нормальный вид, распылились в реголиты за несчитанные эры.' },
        { languageId: 1, sentenceText: '09 Клад был глубоко под землей, под сетью переходов, в залитой темнотой комнате.' },
        { languageId: 1, sentenceText: '10 Информация на квантовом уровне, повреждений нет.' },
        { languageId: 1, sentenceText: '11 Прошло, быть может, миллиардов пять лет, как этот архив ушел со всех сетей.' },
        { languageId: 1, sentenceText: '12 Проклятие фараона – комический образ из собственной истории человечества, давно забытый.' },
        { languageId: 1, sentenceText: '13 * ' },
        { languageId: 1, sentenceText: '14 Они смеялись при этих словах, смеялись от радости, найдя сокровище… и все же твердо решили действовать осторожно.' },
        { languageId: 1, sentenceText: '15 Им предстояло прожить тут от года до пяти, маленькой группе со Страума – археопрограммисты, их семьи и школа для детей.' },
        { languageId: 1, sentenceText: '16 От года до пяти, чтобы подобрать протоколы, снять сливки и выяснить источник клада в пространстве и времени, узнать один-другой секрет, который обогатит царство Страума.' },
        { languageId: 1, sentenceText: '17 А когда закончится работа, место можно будет продать, быть может, построить сетевую связь (но это вряд ли – место это за Краем, и кто знает, какая Сила может наложить лапу на эту находку).' },
        { languageId: 1, sentenceText: '18 * ' },
        { languageId: 1, sentenceText: '19 Так что сейчас тут был крошечный поселок, прозванный жителями Верхняя Лаборатория.' },
        { languageId: 1, sentenceText: '20 Ничего особенного – люди возились со старой библиотекой.' },
        { languageId: 1, sentenceText: '21 При имеющейся автоматике дело безопасное, чистое и простенькое.' },
        { languageId: 1, sentenceText: '22 Библиотека не была живым существом и даже не была автоматизирована (что в этих местах могло значить много больше, куда больше, чем быть человеком).' },
        { languageId: 1, sentenceText: '23 Люди собирались смотреть и выбирать и быть осторожными, чтобы не обжечься.' },
        { languageId: 1, sentenceText: '24  * ' },
        { languageId: 1, sentenceText: '25 Люди устраивают пожары и играют с пламенем.' },
        { languageId: 1, sentenceText: '26 * ' },
        { languageId: 1, sentenceText: '27 Архив проинформировал автоматику.' },
        { languageId: 1, sentenceText: '28 Построились структуры данных, стали выполняться рецепты.' },
        { languageId: 1, sentenceText: '29 Возникла локальная сеть, быстрее, чем в любом месте на Страуме, но с гарантией безопасности.' },
        { languageId: 1, sentenceText: '30 Добавлялись узлы, модифицируемые другими рецептами.' },
        { languageId: 1, sentenceText: '31 Архив был дружественным, иерархия ключей выстраивалась и вела исследователей.' },
        { languageId: 1, sentenceText: '32 Это открытие прославит сам Страум.' }
    ],
    sentencesOnPageTop: 10,
    sentencesCount: [777, 888], //engSentencesCount: 777, rusSentencesCount: 888
    emptyVariable: null,
    isTextLoaded: [false, false],
    creativeArrayLanguageId: [0, 1], //engLanguageId = 0; rusLanguageId = 1;
    bookTitle: [
        { languageId: 0, authorName: '1', bookTitle: '1' },
        { languageId: 1, authorName: '1', bookTitle: '1' }
    ],
    buttonsTextsParts: ['Load English Text -/', 'Load Russian Text -/'],
    loadedTextTitle: ['You loaded English book --> ', 'You loaded Russian book--> '],
    isFetching: false
}

let lastSentenceNumber = initialState.engSentences.length;
initialState.lastSentenceNumber = lastSentenceNumber;
let sentencesOnPageTop = initialState.sentencesOnPageTop;
initialState.readingSentenceNumber = sentencesOnPageTop;
let engEmptyLines = Array(sentencesOnPageTop).fill({ languageId: 0, sentenceText: '-' });
let rusEmptyLines = Array(sentencesOnPageTop).fill({ languageId: 1, sentenceText: '-' });
initialState.engSentences = engEmptyLines.concat(initialState.engSentences).concat(engEmptyLines);
initialState.rusSentences = rusEmptyLines.concat(initialState.rusSentences).concat(rusEmptyLines); // emptu lines on top - (text) - empty lines on bottom

let n = 1;
initialState.engSentences = initialState.engSentences.map(u => ({ ...u, id: n++ }));
//n = 1; - server needs id-s without doubling
initialState.rusSentences = initialState.rusSentences.map(u => ({ ...u, id: n++ }));

const uploadBooksReducer = (state = initialState, action) => {

    switch (action.type) {
        case TOGGLE_IS_LOADING: {
            /* return { ...state, isEngLoaded: action.isEngLoaded } */
            let stateCopy = { ...state };
            stateCopy.isTextLoaded = { ...state.isTextLoaded };
            stateCopy.isTextLoaded[action.languageId] = action.isTextLoaded;
            return stateCopy;
        }
        case SET_SENTENCES_COUNT: {
            let stateCopy = { ...state };
            stateCopy.sentencesCount = { ...state.sentencesCount };
            stateCopy.sentencesCount[action.languageId] = action.count;
            return stateCopy;
        }
        case SET_BOOK_TITLE: {
            let stateCopy = { ...state };
            stateCopy.bookTitle = { ...state.bookTitle };
            switch (action.languageId) {
                case 0:
                    stateCopy.bookTitle[action.languageId] = stateCopy.engTextTitle[action.bookId];
                    return stateCopy;
                case 1:
                    stateCopy.bookTitle[action.languageId] = stateCopy.rusTextTitle[action.bookId];
                    return stateCopy;
            }
        }
        case TOGGLE_IS_FETCHING: {
            return { ...state, isFetching: action.isFetching };
        }
        default:
            return state;
    }
}

export const toggleIsLoading = (isTextLoaded, languageId) => ({ type: TOGGLE_IS_LOADING, isTextLoaded, languageId });
export const setSentencesCount = (count, languageId) => ({ type: SET_SENTENCES_COUNT, count, languageId });
export const setBookTitle = (bookId, languageId) => ({ type: SET_BOOK_TITLE, bookId, languageId });
export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default uploadBooksReducer;
