import { useEffect, useState } from 'react';
import { countryApi } from '../services/officeApi';

// Country list once, and the states of the chosen country.
export function useCountryStates(session, countryId) {
    const [countries, setCountries] = useState([]);
    const [states, setStates] = useState([]);

    useEffect(() => {
        countryApi.fetchCountries(session).then(setCountries).catch(() => setCountries([]));
    }, [session]);

    useEffect(() => {
        if (!countryId) {
            setStates([]);
            return;
        }
        let current = true;
        countryApi.fetchStates(session, Number(countryId))
            .then((list) => { if (current) setStates(list); })
            .catch(() => { if (current) setStates([]); });
        return () => { current = false; };
    }, [session, countryId]);

    return {
        countryOptions: countries.map((item) => ({ value: String(item.countryId), label: item.country })),
        stateOptions: states.map((item) => ({ value: String(item.stateId), label: item.state })),
        countryCodeOf: (id) => {
            const match = countries.find((item) => String(item.countryId) === String(id));
            return match && match.countryCode ? match.countryCode : '';
        },
    };
}
